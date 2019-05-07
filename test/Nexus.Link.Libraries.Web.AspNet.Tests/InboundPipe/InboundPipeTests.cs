using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Text.RegularExpressions;
#else
using System.Net.Http;
#endif

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe
{
    [TestClass]
    public class InboundPipeTests
    {
        private static int _logCounter;
        private static string _foundCorrelationId;
        private static Tenant _foundClientTenant;

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(InboundPipeTests).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
            FulcrumApplication.Context.LeverConfiguration = null;
        }

        [TestMethod]
        public async Task SaveClientTenantOldPrefixSuccess()
        {
#if NETCOREAPP
            var saveConfigHandler =
                new SaveClientTenant(async context =>
                {
                    _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                    await Task.CompletedTask;
                }, SaveClientTenant.LegacyVersionPrefix);
#else
            var mockHandler = new Mock<HttpMessageHandler>();
            var saveConfigHandler = new SaveClientTenant(SaveClientTenant.LegacyVersionPrefix)
            {
                InnerHandler = new GetContextTestHandler
                {
                    InnerHandler = mockHandler.Object
                }
            };
            var invoker = new HttpMessageInvoker(saveConfigHandler);
#endif

            foreach (var entry in new Dictionary<Tenant, string>
            {
                {new Tenant("smoke-testing-company", "ver"), "https://v-mock.org/v2/smoke-testing-company/ver/"},
                {
                    new Tenant("smoke-testing-company", "local"),
                    "https://v-mock.org/api/v-pa1/smoke-testing-company/local/"
                },
                {
                    new Tenant("fulcrum", "prd"),
                    "https://prd-fulcrum-fundamentals.azurewebsites.net/api/v1/fulcrum/prd/ServiceMetas/ServiceHealth"
                },
                {
                    new Tenant("fulcrum", "ver"),
                    "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/fulcrum/ver/ServiceMetas/ServiceHealth"
                }
            })
            {
                _foundClientTenant = null;
                var expectedTenant = entry.Key;
#if NETCOREAPP
                var context = new DefaultHttpContext();
                SetRequest(context, entry.Value);
                await saveConfigHandler.InvokeAsync(context);
#else
                var request = new HttpRequestMessage(HttpMethod.Get, entry.Value);
                await invoker.SendAsync(request, CancellationToken.None);
#endif
                Assert.AreEqual(expectedTenant, _foundClientTenant,
                    $"Could not find tenant '{expectedTenant}' from url '{entry.Value}'. Found {_foundClientTenant}");
            }
        }

        [TestMethod]
        public async Task SaveClientTenantOldPrefixAcceptsFalsePositives()
        {
#if NETCOREAPP
            var saveConfigHandler =
                new SaveClientTenant(async ctx =>
                {
                    _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                    await Task.CompletedTask;
                }, SaveClientTenant.LegacyVersionPrefix);
#else
            var mockHandler = new Mock<HttpMessageHandler>();
            var saveConfigHandler = new SaveClientTenant(SaveClientTenant.LegacyVersionPrefix)
            {
                InnerHandler = new GetContextTestHandler
                {
                    InnerHandler = mockHandler.Object
                }
            };
            var invoker = new HttpMessageInvoker(saveConfigHandler);
#endif

 
                _foundClientTenant = null;
            var url = "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/false/positive/ServiceMetas/ServiceHealth";
#if NETCOREAPP
            var context = new DefaultHttpContext();
            SetRequest(context, url);
                await saveConfigHandler.InvokeAsync(context);
#else
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                await invoker.SendAsync(request, CancellationToken.None);
#endif
            Assert.IsNotNull(_foundClientTenant);
        }

        [TestMethod]
        public async Task SaveClientTenantNewPrefixSuccess()
        {
#if NETCOREAPP
            var saveConfigHandler =
                new SaveClientTenant(async context =>
                {
                    _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                    await Task.CompletedTask;
                }, SaveClientTenant.ApiVersionTenantPrefix);
#else
            var mockHandler = new Mock<HttpMessageHandler>();
            var saveConfigHandler = new SaveClientTenant(SaveClientTenant.ApiVersionTenantPrefix)
            {
                InnerHandler = new GetContextTestHandler
                {
                    InnerHandler = mockHandler.Object
                }
            };
            var invoker = new HttpMessageInvoker(saveConfigHandler);
#endif

            foreach (var entry in new Dictionary<Tenant, string>
            {
                {new Tenant("smoke-testing-company", "ver"), "https://v-mock.org/api/v2/Tenant/smoke-testing-company/ver/"},
                {
                    new Tenant("smoke-testing-company", "local"),
                    "https://v-mock.org/api/v-pa1/Tenant/smoke-testing-company/local/"
                },
                {
                    new Tenant("fulcrum", "prd"),
                    "https://prd-fulcrum-fundamentals.azurewebsites.net/api/v1/Tenant/fulcrum/prd/ServiceMetas/ServiceHealth"
                },
                {
                    new Tenant("fulcrum", "ver"),
                    "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/Tenant/fulcrum/ver/ServiceMetas/ServiceHealth"
                }
            })
            {
                _foundClientTenant = null;
                var expectedTenant = entry.Key;
#if NETCOREAPP
                var context = new DefaultHttpContext();
                SetRequest(context, entry.Value);
                await saveConfigHandler.InvokeAsync(context);
#else
                var request = new HttpRequestMessage(HttpMethod.Get, entry.Value);
                await invoker.SendAsync(request, CancellationToken.None);
#endif
                Assert.AreEqual(expectedTenant, _foundClientTenant,
                    $"Could not find tenant '{expectedTenant}' from url '{entry.Value}'. Found {_foundClientTenant}");
            }
        }

        [TestMethod]
        public async Task SaveClientTenantNewPrefixDetectsFalsePositives()
        {
#if NETCOREAPP
            var saveConfigHandler =
                new SaveClientTenant(async ctx =>
                {
                    _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                    await Task.CompletedTask;
                }, SaveClientTenant.ApiVersionTenantPrefix);
#else
            var mockHandler = new Mock<HttpMessageHandler>();
            var saveConfigHandler = new SaveClientTenant(SaveClientTenant.ApiVersionTenantPrefix)
            {
                InnerHandler = new GetContextTestHandler
                {
                    InnerHandler = mockHandler.Object
                }
            };
            var invoker = new HttpMessageInvoker(saveConfigHandler);
#endif


            _foundClientTenant = null;
            const string url = "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/false/positive/ServiceMetas/ServiceHealth";
#if NETCOREAPP
            var context = new DefaultHttpContext();
            SetRequest(context, url);
            await saveConfigHandler.InvokeAsync(context);
#else
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                await invoker.SendAsync(request, CancellationToken.None);
#endif
            Assert.IsNull(_foundClientTenant);
        }

#if NETCOREAPP
        private void SetRequest(DefaultHttpContext context, string url)
        {
            var request = new DefaultHttpRequest(context);
            var match = Regex.Match(url, "^(https?)://([^/]+)(/[^?]+)(\\?.*)?$");
            request.Scheme = match.Groups[1].Value;
            request.Host = new HostString(match.Groups[2].Value);
            request.PathBase = new PathString("/");
            request.Path = new PathString(match.Groups[3].Value);
            request.Method = "GET";
            request.Body = new MemoryStream();
            request.QueryString = new QueryString(match.Groups[4].Value);
        }
#endif
        [TestMethod]
        public async Task SaveConfigurationFail()
        {
#if NETCOREAPP
            var saveConfigHandler =
                new SaveClientTenant(async context => await Task.CompletedTask, SaveClientTenant.LegacyVersionPrefix);
#else
            var saveConfigHandler = new SaveClientTenant(SaveClientTenant.LegacyVersionPrefix)
            {
                InnerHandler = new Mock<HttpMessageHandler>().Object
            };
            var invoker = new HttpMessageInvoker(saveConfigHandler);
#endif
            FulcrumApplication.Context.ClientTenant = null;
            foreach (var url in new[]
            {
                "http://gooogle.com/",
                "https://anywhere.org/api/v1/eels/"
            })
            {
#if NETCOREAPP
                var context = new DefaultHttpContext();
                SetRequest(context, url);
                await saveConfigHandler.InvokeAsync(context);
#else
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                await invoker.SendAsync(request, CancellationToken.None);
#endif
                Assert.IsNull(FulcrumApplication.Context.ClientTenant);
            }
        }

        /// <summary>
        /// This represents the preferred order in the pipe when saving configuration and propagating correlation id:
        ///
        /// 1. <see cref="SaveConfiguration"/>
        /// 2. <see cref="SaveCorrelationId"/>
        /// </summary>
        [TestMethod]
        public async Task SavedConfigurationWhenLoggingWithNoCorrelationId()
        {
            // Simulate a pipe of DelegatingHandlers where SaveConfiguration happens first
#if NETCOREAPP
            var innerHandler = new SaveCorrelationId(async context =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                _foundCorrelationId = FulcrumApplication.Context.CorrelationId;
                await Task.CompletedTask;
            });
            var middleHandler = new SaveClientTenantConfiguration(innerHandler.InvokeAsync, new Mock<ILeverServiceConfiguration>().Object);
            var outerHandler =
                new SaveClientTenant(middleHandler.InvokeAsync, SaveClientTenant.LegacyVersionPrefix);
#else
            var outerHandler = new SaveClientTenant(SaveClientTenant.LegacyVersionPrefix)
            {
                InnerHandler = new SaveClientTenantConfiguration(new Mock<ILeverServiceConfiguration>().Object)
                {
                    InnerHandler = new SaveCorrelationId
                    {
                        InnerHandler = new GetContextTestHandler()
                        {
                            InnerHandler = new Mock<HttpMessageHandler>().Object
                        }
                    }
                }
            };
#endif

            // Define Organization/Environment for the uri path
            const string org = "my-org";
            const string env = "prd";

            // Invoke a request and get back the tenant that was logged on
            var loggedTenant = await SavedConfigurationPlayingWithSaveCorrelationId(
#if NETCOREAPP
                outerHandler.InvokeAsync,
#else
                outerHandler,
#endif
                org, env);

            Assert.IsNotNull(_foundCorrelationId, "CorrelationId should have been created");
            Assert.AreEqual(_foundClientTenant, loggedTenant,
                $"Expected the tenant on the value provider ('{_foundClientTenant}') to equal the logged tenant ('{loggedTenant}')");
        }

        private async Task<Tenant> SavedConfigurationPlayingWithSaveCorrelationId(

#if NETCOREAPP
            RequestDelegate invokeDelegate,
#else
            DelegatingHandler outerHandler,
#endif
            string organization, string environment)
        {
            // Setup a mocked logger as the FullLogger so that we have full control
            var mockLogger = new Mock<ISyncLogger>();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;

            // We want to register when the LogAsync method is invoked and catch the tenant
            var loggingCalled = new ManualResetEvent(false);
            Tenant loggedTenant = null;
            mockLogger.Setup(x => x.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord x) =>
                {
                    loggedTenant = FulcrumApplication.Context.ClientTenant;
                    loggingCalled.Set();
                })
                // Support for verifying that the method has been invoked
                .Verifiable();

            var url = $"https://v-mock.org/v2-alpha/{organization}/{environment}/Entity";

#if NETCOREAPP
            var context = new DefaultHttpContext();
            SetRequest(context, url);
            await invokeDelegate(context);

#else
// Simulate an incoming request with a an inbound pipe
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            await invoker.SendAsync(request, CancellationToken.None);
#endif

            // Wait for the LogAsync method to be invoked
            Assert.IsTrue(loggingCalled.WaitOne(TimeSpan.FromSeconds(3)));
            mockLogger.Verify();

            // Tell the audience which tenant was logged on
            return loggedTenant;
        }

        /// <summary>
        /// Make sure <see cref="SaveConfiguration"/> propagates correlation id in case of any logging. 
        /// </summary>
        [TestMethod]
        public async Task SavedConfigurationHandlesNoCorrelationId()
        {
            // Setup a mocked logger as the FullLogger so that we have full control
            var mockLogger = new Mock<ISyncLogger>();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            mockLogger.Setup(x => x.LogSync(It.IsAny<LogRecord>())).Verifiable();

            // The expected correlation id propagated as a request header
            const string corrId = "Qorrr";

            var leverConfig = new Mock<ILeverServiceConfiguration>();
#if NETCOREAPP
            var innerHandler =
                new SaveClientTenantConfiguration(async c =>
                {
                    _foundCorrelationId = FulcrumApplication.Context.CorrelationId;
                    await Task.CompletedTask;
                }, leverConfig.Object);
            var outerHandler = new SaveClientTenant(innerHandler.InvokeAsync, SaveClientTenant.LegacyVersionPrefix);
#else
            // Simulate a pipe of DelegatingHandlers
            var outerHandler = new SaveClientTenant(SaveClientTenant.LegacyVersionPrefix)
            {
                InnerHandler = new SaveClientTenantConfiguration(new Mock<ILeverServiceConfiguration>().Object)
                {
                    InnerHandler = new GetContextTestHandler
                    {
                        InnerHandler = new Mock<HttpMessageHandler>().Object
                    }
                }
            };
#endif
            var url = "https://v-mock.org/v2/smoke-testing-company/ver/";

            // Simulate an incoming request
#if NETCOREAPP
            var context = new DefaultHttpContext();
            SetRequest(context, url);
            context.Request.Headers.Add("X-Correlation-ID", corrId);
            await outerHandler.InvokeAsync(context);
#else
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Correlation-ID", corrId);
            await invoker.SendAsync(request, CancellationToken.None);
#endif

            // Check that LogAsync has NOT been invoked
            mockLogger.VerifyNoOtherCalls();

            Assert.AreEqual(corrId, _foundCorrelationId,
                "When SaveConfiguration is run before SaveCorrelationId, we still expect X-Correlation-ID header to be handled");
        }

        [TestMethod]
        public async Task BatchLogs()
        {
            _logCounter = 0;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    Interlocked.Increment(ref _logCounter);
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = new BatchLogger(mockLogger.Object);

#if NETCOREAPP
            var doLogging = new LogFiveTimesHandler(async c => await Task.CompletedTask);
            var saveConfigHandler = new BatchLogs(doLogging.InvokeAsync, LogSeverityLevel.Information, LogSeverityLevel.Warning);
            var context = new DefaultHttpContext();
            await saveConfigHandler.InvokeAsync(context);
#else
            var handler = new BatchLogs(LogSeverityLevel.Information, LogSeverityLevel.Warning)
            {
                InnerHandler = new LogFiveTimesHandler()
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://v-mock.org/v2/smoke-testing-company/ver");
            await invoker.SendAsync(request, CancellationToken.None);
#endif
            mockLogger.Verify();
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task LogRequestAndResponseMustNotBeBeforeBatchLogs()
        {
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var innerHandler = new BatchLogs(async ctx => await Task.CompletedTask);
            var outerHandler =
                new LogRequestAndResponse(innerHandler.InvokeAsync);
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var outerHandler = new LogRequestAndResponse()
            {
                InnerHandler = new BatchLogs
                {
                    InnerHandler = new GetContextTestHandler()
                    {
                        InnerHandler = new Mock<HttpMessageHandler>().Object
                    }
                }
            };
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif

            try
            {
#if NETCOREAPP
                await outerHandler.InvokeAsync(context);
#else
                await invoker.SendAsync(request, CancellationToken.None);
#endif
                Assert.Fail("Expected an exception");
            }
            catch (FulcrumContractException e)
            {
                Assert.IsTrue(e.Message.Contains("must not precede"));
            }
            catch (Exception e)
            {
                Assert.Fail(
                    $"Expected an exception of type {nameof(FulcrumContractException)}, but caught exception {e.GetType().FullName}");
            }
        }

        /// <summary>
        /// SaveCorrelationId must not come before SaveClientTenantConfiguration in the pipe
        /// </summary>
        [TestMethod]
        public async Task SaveCorrelationIdMustNotBeBeforeSaveClientTenantConfiguration()
        {
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var innerHandler = new SaveClientTenantConfiguration(async ctx => await Task.CompletedTask, new Mock<ILeverServiceConfiguration>().Object);
            var middleHandler =
                new SaveClientTenant(innerHandler.InvokeAsync, SaveClientTenant.LegacyVersionPrefix);
            var outerHandler =
                new SaveCorrelationId(middleHandler.InvokeAsync);
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var outerHandler = new SaveCorrelationId
            {
                InnerHandler = new SaveClientTenant(SaveClientTenant.LegacyVersionPrefix)
                {
                    InnerHandler = new SaveClientTenantConfiguration(new Mock<ILeverServiceConfiguration>().Object)
                    {
                        InnerHandler = new GetContextTestHandler()
                        {
                            InnerHandler = new Mock<HttpMessageHandler>().Object
                        }
                    }
                }
            };
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif

            try
            {
#if NETCOREAPP
                await outerHandler.InvokeAsync(context);
#else
                await invoker.SendAsync(request, CancellationToken.None);
#endif
                Assert.Fail("Expected an exception");
            }
            catch (FulcrumContractException e)
            {
                Assert.IsTrue(e.Message.Contains("must not precede"));
            }
            catch (Exception e)
            {
                Assert.Fail(
                    $"Expected an exception of type {nameof(FulcrumContractException)}, but caught exception {e.GetType().FullName}");
            }
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task SaveClientTenantMustRunBeforeSaveClientTenantConfiguration()
        {
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
#if NETCOREAPP
            var outerHandler =
                new SaveClientTenantConfiguration(async ctx => await Task.CompletedTask,
                    new Mock<ILeverServiceConfiguration>().Object);
            var context = new DefaultHttpContext();
            SetRequest(context, url);
#else
            var outerHandler = new SaveClientTenantConfiguration(new Mock<ILeverServiceConfiguration>().Object)
            {
                InnerHandler = new GetContextTestHandler()
                {
                    InnerHandler = new Mock<HttpMessageHandler>().Object
                }
            };
            var invoker = new HttpMessageInvoker(outerHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, url);
#endif

            try
            {
#if NETCOREAPP
                await outerHandler.InvokeAsync(context);
#else
                await invoker.SendAsync(request, CancellationToken.None);
#endif
                Assert.Fail("Expected an exception");
            }
            catch (FulcrumContractException e)
            {
                Assert.IsTrue(e.Message.Contains("must be preceded by"));
            }
            catch (Exception e)
            {
                Assert.Fail(
                    $"Expected an exception of type {nameof(FulcrumContractException)}, but caught exception {e.GetType().FullName}");
            }
        }

#if NETCOREAPP
        private class LogFiveTimesHandler
        {
            private readonly RequestDelegate _next;

            /// <inheritdoc />
            public LogFiveTimesHandler(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                Log.LogVerbose("Verbose");
                Assert.AreEqual(0, _logCounter);
                Log.LogInformation("Information");
                Assert.AreEqual(0, _logCounter);
                Log.LogWarning("Warning");
                Assert.AreEqual(3, _logCounter);
                Log.LogError("Error");
                Assert.AreEqual(4, _logCounter);
                Log.LogCritical("Critical");
                Assert.AreEqual(5, _logCounter);
                await _next(context);
            }
        }
#else
        private class LogFiveTimesHandler : DelegatingHandler
        {
            /// <inheritdoc />
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                Log.LogVerbose("Verbose");
                Assert.AreEqual(0, _logCounter);
                Log.LogInformation("Information");
                Assert.AreEqual(0, _logCounter);
                Log.LogWarning("Warning");
                Assert.AreEqual(3, _logCounter);
                Log.LogError("Error");
                Assert.AreEqual(4, _logCounter);
                Log.LogCritical("Critical");
                Assert.AreEqual(5, _logCounter);
                return Task.FromResult((HttpResponseMessage)null);
            }
        }
        private class GetContextTestHandler : DelegatingHandler
        {
            /// <inheritdoc />
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                _foundCorrelationId = FulcrumApplication.Context.CorrelationId;
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.FromResult((HttpResponseMessage)null);
            }
        }
#endif
    }
}
