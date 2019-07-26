#if NETCOREAPP
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Web.AspNet.Pipe.Inbound;
using Nexus.Link.Libraries.Web.Platform.Authentication;
using Swashbuckle.AspNetCore.Swagger;

namespace Nexus.Link.Libraries.Web.AspNet.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class StartupHelperBase : IValidatable
    {
        /// <summary>
        /// The base URL to the authentication service for authenticating your app vs. Nexus Link.
        /// </summary>
        /// <remarks>Only useful for the Business API, not for Nexus Adapters.</remarks>
        protected static string NexusLinkAuthenticationBaseUrl { get; set; }

        /// <summary>
        /// The base URL to the Nexus Link business events service.
        /// </summary>
        /// <remarks>Only useful for the Business API, not for Nexus Adapters.</remarks>
        protected static string BusinessEventsBaseUrl { get; set; }

        /// <summary>
        /// A token generator for authenticating your app vs. Nexus Link.
        /// </summary>
        /// <remarks>Only useful for the Business API, not for Nexus Adapters.</remarks>
        protected static ITokenRefresherWithServiceClient NexusLinkTokenRefresher { get; set; }

        /// <summary>
        /// The base URL to the authentication service for authenticating within your platform.
        /// </summary>
        public static string LocalAuthenticationBaseUrl { get; protected set; }

        /// <summary>
        /// A token generator for authenticating between adapters and the business API.
        /// </summary>
        public ITokenRefresherWithServiceClient LocalTokenRefresher { get; set; }

        /// <summary>
        /// The configuration from the Startup constructor.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 
        /// </summary>
        protected bool IsBusinessApi { get; }

        /// <summary>
        /// Use this constructor in the Startup constructor.
        /// </summary>
        /// <param name="configuration">Take this from the Startup constructor.</param>
        /// <param name="isBusinessApi">Should be false for adapters and true for the business API.</param>
        protected StartupHelperBase(IConfiguration configuration, bool isBusinessApi)
        {
            Configuration = configuration;
            IsBusinessApi = isBusinessApi;
        }

        /// <summary>
        /// A user friendly name, e.g. "Salesforce adapter"
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// The technical name of this adapter, e.g. "Salesforce.NexusAdapter"
        /// </summary>
        /// <remarks>
        /// Will be used to generate other technical names like the file for Swagger documentation.</remarks>
        public virtual string TechnicalName { get; set; }

        /// <summary>
        /// The version of the API. To be used in the Open API (swagger).
        /// </summary>
        public virtual string ApiVersion { get; set; } = "v1";

        /// <summary>
        /// The .NET Core version. Defaults to the latest.
        /// </summary>
        public virtual CompatibilityVersion CompatibilityVersion { get; set; } = CompatibilityVersion.Latest;


        /// <summary>
        /// The name of the XML file. Created based on the <see cref="TechnicalName"/>.
        /// </summary>
        public virtual string GetCommentsFilePath()
        {
            var xmlFile = $"{TechnicalName}.Services.xml";
            return Path.Combine(AppContext.BaseDirectory, xmlFile);
        }

        /// <summary>
        /// Use this method in the Startup.ConfigureServices method and use the parameter from that method.
        /// </summary>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            try
            {
                InternalContract.RequireValidated(this, GetType().FullName);
                InitialServiceConfiguration(services);
                DependencyInjectServices(services);
                Log.LogInformation($"{nameof(StartupHelperBase)}.{nameof(ConfigureServices)} succeeded.");
            }
            catch (Exception e)
            {
                Log.LogCritical(
                    $"{nameof(StartupHelperBase)}.{nameof(ConfigureServices)} failed. Application {TechnicalName} needs to be restarted.",
                    e);
                throw;
            }
        }

        /// <summary>
        /// Use this method in the Startup.Configure method and use the parameters from that method.
        /// </summary>
        public virtual void ConfigureMiddleware(IApplicationBuilder app, IHostingEnvironment env)
        {
            try
            {
                InternalContract.RequireValidated(this, GetType().FullName);
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    app.UseHsts();
                }
                app.UseHttpsRedirection();
                ConfigureNexusLinkMiddleware(app, env);
                ConfigureSwaggerMiddleware(app, env);
                ConfigureBusinessApiMiddleware(app, env);
                ConfigureAppMiddleware(app, env);
                app.UseMvc();
                Log.LogInformation($"{nameof(StartupHelperBase)}.{nameof(ConfigureMiddleware)} succeeded.");
            }
            catch (Exception e)
            {
                Log.LogCritical($"{nameof(StartupHelperBase)}.{nameof(ConfigureMiddleware)} failed. Application {TechnicalName} needs to be restarted.", e);
                throw;
            }
        }

        #region Configure Services
        /// <summary>
        /// Makes the initial most urgent Nexus settings - application properties and logging.
        /// </summary>
        /// <remarks>
        /// Assumes that there is a "FulcrumApplication" section in the app settings file.
        /// </remarks>
        /// <param name="services">The parameter from Startup.ConfigureServices.</param>
        protected virtual void InitialNexusLinkConfiguration(IServiceCollection services)
        {
            var configurationSection = Configuration.GetSection("FulcrumApplication");
            FulcrumAssert.IsNotNull(configurationSection);
            Application.FulcrumApplicationHelper.WebBasicSetup(configurationSection);
        }

        /// <summary>
        /// Initial settings that is needed for your environment
        /// </summary>
        /// <param name="services">The parameter from Startup.ConfigureServices.</param>
        protected abstract void InitialLocalConfiguration(IServiceCollection services);

        /// <summary>
        /// Configure Swagger
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        protected virtual void InitialSwaggerConfiguration(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion);
                c.SwaggerDoc(ApiVersion, new Info {Title = DisplayName, Version = ApiVersion});
                c.AddSecurityDefinition("Bearer",
                    new ApiKeyScheme
                    {
                        In = "header",
                        Description = "Please enter into field the word 'Bearer' following by space and JWT",
                        Name = "Authorization",
                        Type = "apiKey"
                    });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", Enumerable.Empty<string>()},
                });

                c.IncludeXmlComments(GetCommentsFilePath());
            });
        }

        /// <summary>
        /// Injects the services from Nexus.Services.Implementation
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        protected abstract void DependencyInjectNexusServices(IServiceCollection services);

        /// <summary>
        /// This is where the business API injects its service SDK for the adapters.
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        protected virtual void DependencyInjectBusinessApiServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// This is where the application injects its own services.
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        protected abstract void DependencyInjectAppServices(IServiceCollection services);

        /// <summary>
        /// Convenience method to catch dependency injection exceptions and log them as critical errors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="dependencyInjection"></param>
        /// <returns></returns>
        public virtual T ValidateDependencyInjection<T>(IServiceProvider provider,
            Func<IServiceProvider, T> dependencyInjection)
        {
            try
            {
                return dependencyInjection(provider);
            }
            catch (Exception e)
            {
                Log.LogCritical($"Dependency injection failed for {typeof(T).FullName}.", e);
                throw;
            }
        }

        private void InitialServiceConfiguration(IServiceCollection services)
        {
            InitialNexusLinkConfiguration(services);
            InitialLocalConfiguration(services);
            InitialSwaggerConfiguration(services);
        }

        private void DependencyInjectServices(IServiceCollection services)
        {
            if (IsBusinessApi)
            {
                DependencyInjectNexusServices(services);
            }
            else
            {
                DependencyInjectBusinessApiServices(services);
            }
            DependencyInjectAppServices(services);
        }
        #endregion

        #region Configure

        /// <summary>
        /// Configure the Nexus Link middleware 
        /// </summary>
        /// <remarks>
        /// Configures:
        /// <see cref="SaveCorrelationId"/>,
        /// <see cref="BatchLogs"/>,
        /// <see cref="LogRequestAndResponse"/>,
        /// <see cref="ExceptionToFulcrumResponse"/>
        /// </remarks>
        protected virtual void ConfigureNexusLinkMiddleware(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Get the correlation ID from the request header and store it in FulcrumApplication.Context
            app.UseNexusSaveCorrelationId();
            // Start and stop a batch of logs, see also Nexus.Link.Libraries.Core.Logging.BatchLogger.
            app.UseNexusBatchLogs();
            // Log all requests and responses
            app.UseNexusLogRequestAndResponse();
            // Convert exceptions into error responses (HTTP status codes 400 and 500)
            app.UseNexusExceptionToFulcrumResponse();
        }

        /// <summary>
        /// Configure the Swagger middleware
        /// </summary>
        protected virtual void ConfigureSwaggerMiddleware(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{FulcrumApplication.Setup.Name} {ApiVersion}");
            });
        }

        /// <summary>
        /// Configure optional middleware from the platform.
        /// </summary>
        protected abstract void ConfigureBusinessApiMiddleware(IApplicationBuilder app, IHostingEnvironment env);


        /// <summary>
        /// Configure optional middleware unique for this app.
        /// </summary>
        protected abstract void ConfigureAppMiddleware(IApplicationBuilder app, IHostingEnvironment env);
        #endregion

        /// <inheritdoc />
        public void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(DisplayName, nameof(DisplayName), errorLocation);
            FulcrumValidate.IsNotNull(TechnicalName, nameof(TechnicalName), errorLocation);
            FulcrumValidate.IsNotNull(ApiVersion, nameof(TechnicalName), ApiVersion);
            FulcrumValidate.IsNotNull(TechnicalName, nameof(TechnicalName), errorLocation);
            if (IsBusinessApi)
            {
                FulcrumValidate.IsNotNullOrWhiteSpace(NexusLinkAuthenticationBaseUrl, nameof(NexusLinkAuthenticationBaseUrl), errorLocation);
                FulcrumValidate.IsNotNullOrWhiteSpace(BusinessEventsBaseUrl, nameof(BusinessEventsBaseUrl), errorLocation);
                FulcrumValidate.IsNotNull(NexusLinkTokenRefresher, nameof(NexusLinkTokenRefresher), errorLocation);
            }
            FulcrumValidate.IsNotNullOrWhiteSpace(LocalAuthenticationBaseUrl, nameof(LocalAuthenticationBaseUrl), errorLocation);
            FulcrumValidate.IsNotNull(LocalTokenRefresher, nameof(LocalTokenRefresher), errorLocation);
        }
    }
}
#endif