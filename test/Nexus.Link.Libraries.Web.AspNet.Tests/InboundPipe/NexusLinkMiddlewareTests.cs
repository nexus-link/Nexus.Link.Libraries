#if NETCOREAPP
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Web.AspNet.Pipe;
using Nexus.Link.Libraries.Web.Error.Logic;

namespace Nexus.Link.Libraries.Web.AspNet.Tests.InboundPipe
{
    [TestClass]
    public class NexusLinkMiddlewareTests
    {
        private static int _logCounter;
        private static string _foundCorrelationId;
        private static Tenant _foundClientTenant;

        [TestInitialize]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInitialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(NexusLinkMiddlewareTests).FullName);
            FulcrumApplication.Context.CorrelationId = null;
            FulcrumApplication.Context.ClientTenant = null;
            FulcrumApplication.Context.LeverConfiguration = null;
        }

        [TestMethod]
        public async Task SaveClientTenantOldPrefixSuccess()
        {
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureSaveClientTenantToContext = true,
                SaveClientTenantPrefix = NexusLinkMiddleWareOptions.LegacyVersionPrefix
            };
            var handler = new NexusLinkMiddleware(context =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.CompletedTask;
            }, options);
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
                var context = new DefaultHttpContext();
                SetRequest(context, entry.Value);
                await handler.InvokeAsync(context);
                Assert.AreEqual(expectedTenant, _foundClientTenant,
                    $"Could not find tenant '{expectedTenant}' from url '{entry.Value}'. Found {_foundClientTenant}");
            }
        }

        [TestMethod]
        public async Task SaveClientTenantOldPrefixAcceptsFalsePositives()
        {
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureSaveClientTenantToContext = true,
                SaveClientTenantPrefix = NexusLinkMiddleWareOptions.LegacyVersionPrefix
            };
            var handler = new NexusLinkMiddleware(ctx =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.CompletedTask;
            }, options);

            _foundClientTenant = null;
            var url = "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/false/positive/ServiceMetas/ServiceHealth";
            var context = new DefaultHttpContext();
            SetRequest(context, url);
            await handler.InvokeAsync(context);
            Assert.IsNotNull(_foundClientTenant);
        }

        [TestMethod]
        public async Task SaveClientTenantNewPrefixSuccess()
        {
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureSaveClientTenantToContext = true,
                SaveClientTenantPrefix = NexusLinkMiddleWareOptions.ApiVersionTenantPrefix
            };
            var handler = new NexusLinkMiddleware(context =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.CompletedTask;
            }, options);

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
                var context = new DefaultHttpContext();
                SetRequest(context, entry.Value);
                await handler.InvokeAsync(context);
                Assert.AreEqual(expectedTenant, _foundClientTenant,
                    $"Could not find tenant '{expectedTenant}' from url '{entry.Value}'. Found {_foundClientTenant}");
            }
        }

        [TestMethod]
        public async Task SaveClientTenantNewPrefixDetectsFalsePositives()
        {
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureSaveClientTenantToContext = true,
                SaveClientTenantPrefix = NexusLinkMiddleWareOptions.ApiVersionTenantPrefix
            };
            var handler = new NexusLinkMiddleware(ctx =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                return Task.CompletedTask;
            }, options);


            _foundClientTenant = null;
            const string url = "https://ver-fulcrum-fundamentals.azurewebsites.net/api/v1/false/positive/ServiceMetas/ServiceHealth";
            var context = new DefaultHttpContext();
            SetRequest(context, url);
            await handler.InvokeAsync(context);
            Assert.IsNull(_foundClientTenant);
        }

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

        [TestMethod]
        public async Task SaveConfigurationFail()
        {
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureSaveClientTenantToContext = true,
                SaveClientTenantPrefix = NexusLinkMiddleWareOptions.LegacyVersionPrefix
            };
            var handler = new NexusLinkMiddleware(ctx => Task.CompletedTask, options);
            FulcrumApplication.Context.ClientTenant = null;
            foreach (var url in new[]
            {
                "http://gooogle.com/",
                "https://anywhere.org/api/v1/eels/"
            })
            {
                var context = new DefaultHttpContext();
                SetRequest(context, url);
                await handler.InvokeAsync(context);
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
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureSaveClientTenantToContext = true,
                SaveClientTenantPrefix = NexusLinkMiddleWareOptions.LegacyVersionPrefix,
                UseFeatureSaveCorrelationIdToContext = true,
                UseFeatureSaveTenantConfigurationToContext = true,
                SaveTenantConfigurationServiceConfiguration = new Mock<ILeverServiceConfiguration>().Object
            };
            var handler = new NexusLinkMiddleware(context =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                _foundCorrelationId = FulcrumApplication.Context.CorrelationId;
                return Task.CompletedTask;
            }, options);

            // Define Organization/Environment for the uri path
            const string org = "my-org";
            const string env = "prd";

            // Invoke a request and get back the tenant that was logged on
            var loggedTenant = await SavedConfigurationPlayingWithSaveCorrelationId(
                handler.InvokeAsync, org, env);

            Assert.IsNotNull(_foundCorrelationId, "CorrelationId should have been created");
            Assert.AreEqual(_foundClientTenant, loggedTenant,
                $"Expected the tenant on the value provider ('{_foundClientTenant}') to equal the logged tenant ('{loggedTenant}')");
        }

        private async Task<Tenant> SavedConfigurationPlayingWithSaveCorrelationId(
            RequestDelegate invokeDelegate, string organization, string environment)
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

            var context = new DefaultHttpContext();
            SetRequest(context, url);
            await invokeDelegate(context);

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
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureSaveClientTenantToContext = true,
                SaveClientTenantPrefix = NexusLinkMiddleWareOptions.LegacyVersionPrefix,
                UseFeatureSaveTenantConfigurationToContext = true,
                SaveTenantConfigurationServiceConfiguration = leverConfig.Object,
                UseFeatureSaveCorrelationIdToContext = true
            };
            var handler = new NexusLinkMiddleware(ctx =>
            {
                _foundClientTenant = FulcrumApplication.Context.ClientTenant;
                _foundCorrelationId = FulcrumApplication.Context.CorrelationId;
                return Task.CompletedTask;
            }, options);
            var url = "https://v-mock.org/v2/smoke-testing-company/ver/";

            // Simulate an incoming request
            var context = new DefaultHttpContext();
            SetRequest(context, url);
            context.Request.Headers.Add("X-Correlation-ID", corrId);
            await handler.InvokeAsync(context);

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
                    Assert.IsTrue(FulcrumApplication.Context.IsInBatchLogger);
                    Interlocked.Increment(ref _logCounter);
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = new BatchLogger(mockLogger.Object);
            FulcrumApplication.Setup.LogSeverityLevelThreshold = LogSeverityLevel.Information;

            var doLogging = new LogFiveTimesHandler(async c => await Task.CompletedTask);
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureBatchLog = true,
                BatchLogThreshold = LogSeverityLevel.Warning
            };
            var handler = new NexusLinkMiddleware(doLogging.InvokeAsync, options);
            var context = new DefaultHttpContext();
            Assert.IsFalse(FulcrumApplication.Context.IsInBatchLogger);
            await handler.InvokeAsync(context);
            Assert.IsFalse(FulcrumApplication.Context.IsInBatchLogger);
            mockLogger.Verify();
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task StatusCode200LogInformation()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger
                .Setup(logger => logger.LogSync(It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                });
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 200);
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureLogRequestAndResponse = true
            };
            var outerHandler = new NexusLinkMiddleware(innerHandler.InvokeAsync, options);
            var context = new DefaultHttpContext();
            SetRequest(context, url);

            await outerHandler.InvokeAsync(context);

            Assert.AreEqual(LogSeverityLevel.Information, highestSeverityLevel);
        }

        /// <summary>
        /// LogRequestAndResponse must not come before BatchLogs in the pipe
        /// </summary>
        [TestMethod]
        public async Task StatusCode400LogWarning()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 400);
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureLogRequestAndResponse = true
            };
            var outerHandler = new NexusLinkMiddleware(innerHandler.InvokeAsync, options);
            var context = new DefaultHttpContext();
            SetRequest(context, url);

            await outerHandler.InvokeAsync(context);

            Assert.AreEqual(LogSeverityLevel.Warning, highestSeverityLevel);
        }

        [TestMethod]
        public async Task StatusCode500LogError()
        {
            var highestSeverityLevel = LogSeverityLevel.None;
            var mockLogger = new Mock<ISyncLogger>();
            mockLogger.Setup(logger =>
                    logger.LogSync(
                        It.IsAny<LogRecord>()))
                .Callback((LogRecord lr) =>
                {
                    if (lr.SeverityLevel > highestSeverityLevel) highestSeverityLevel = lr.SeverityLevel;
                })
                .Verifiable();
            FulcrumApplication.Setup.SynchronousFastLogger = mockLogger.Object;
            const string url = "https://v-mock.org/v2/smoke-testing-company/ver";
            var innerHandler = new ReturnResponseWithPresetStatusCode(async ctx => await Task.CompletedTask, 500);
            var options = new NexusLinkMiddleWareOptions
            {
                UseFeatureLogRequestAndResponse = true
            };
            var outerHandler = new NexusLinkMiddleware(innerHandler.InvokeAsync, options);
            var context = new DefaultHttpContext();
            SetRequest(context, url);

            await outerHandler.InvokeAsync(context);

            Assert.AreEqual(LogSeverityLevel.Error, highestSeverityLevel);
        }

        private class ReturnResponseWithPresetStatusCode
        {
            private readonly int _statusCode;
            private readonly RequestDelegate _next;

            public ReturnResponseWithPresetStatusCode(RequestDelegate next, int statusCode)
            {
                _next = next;
                _statusCode = statusCode;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                context.Response.StatusCode = _statusCode;
                FulcrumException fulcrumException = null;
                if (_statusCode >= 500)
                {
                    fulcrumException = new FulcrumAssertionFailedException("Internal error message");
                }
                else if (_statusCode >= 400)
                {
                    fulcrumException = new FulcrumServiceContractException("Client error message");
                }

                if (_statusCode >= 400)
                {
                    await context.Response.WriteAsync("Test");
                    context.Response.Body = new MemoryStream();
                    context.Response.ContentType = "application/json";
                    var fulcrumError = ExceptionConverter.ToFulcrumError(fulcrumException);
                    var content = ExceptionConverter.ToJsonString(fulcrumError, Formatting.Indented);
                    await context.Response.WriteAsync(content);
                }
            }
        }

        private class LogFiveTimesHandler
        {
            private readonly RequestDelegate _next;

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
    }
}
#endif