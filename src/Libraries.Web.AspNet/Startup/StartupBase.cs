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
using Swashbuckle.AspNetCore.Swagger;

namespace Nexus.Link.Libraries.Web.AspNet.Startup
{
    /// <summary>
    /// Helper class for the different steps in the Startup.cs file.
    /// </summary>
    public abstract class StartupBase : IValidatable
    {
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
        protected StartupBase(IConfiguration configuration, bool isBusinessApi)
        {
            Configuration = configuration;
            IsBusinessApi = isBusinessApi;
        }

        /// <summary>
        /// The technical name of this web app, e.g. "Business API" or "Salesforce.NexusAdapter"
        /// </summary>
        /// <remarks>
        /// Will be used to generate other technical names like the file for Swagger documentation.</remarks>
        protected string TechnicalName { get; set; }

        /// <summary>
        /// The version of the API. To be used in the Open API (swagger).
        /// </summary>
        protected virtual string ApiVersion { get; set; } = "v1";

        /// <summary>
        /// The .NET Core version. Defaults to the latest.
        /// </summary>
        protected virtual CompatibilityVersion CompatibilityVersion { get; set; } = CompatibilityVersion.Latest;

        /// <summary>
        /// This method will be called indirectly from the program's Main method to configure the services
        /// </summary>
        /// <remarks>
        /// Don't override this method unless you really know what you are doing.
        /// First see if the following methods could be good enough for your needs:
        /// Always override <see cref="GetSynchronousFastLogger"/> to establish your preferred way of logging.
        /// Always override <see cref="DependencyInjectServices"/> to inject your own services.
        /// Override <see cref="ConfigureServicesInitialUrgentPart"/> if you have things that needs to be initialized early.
        /// Override <see cref="ConfigureServicesSwagger"/> if you want to change how swagger is set up.
        /// </remarks>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            try
            {
                ConfigureServicesInitialUrgentPart(services);
                FulcrumApplication.ValidateButNotInProduction();
                InternalContract.RequireValidated(this, GetType().FullName);
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion);
                ConfigureServicesSwagger(services);
                DependencyInjectServices(services);
                Log.LogInformation($"{nameof(StartupBase)}.{nameof(ConfigureServices)} succeeded.");
            }
            catch (Exception e)
            {
                Log.LogCritical(
                    $"{nameof(StartupBase)}.{nameof(ConfigureServices)} failed. The application {TechnicalName} needs to be restarted.: {e.Message}",
                    e);
                throw;
            }
        }

        /// <summary>
        /// This method will be called indirectly from the program's Main method to configure the services
        /// </summary>
        /// <remarks>
        /// Don't override this method unless you really know what you are doing.
        /// First see if the following methods could be good enough for your needs:
        /// Override <see cref="ConfigureSwaggerMiddleware"/> if you want to change how swagger is set up.
        /// </remarks>
        public virtual void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            try
            {
                ConfigureAppMiddleware(app, env);
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
                app.UseMvc();
                Log.LogInformation($"{nameof(StartupBase)}.{nameof(Configure)} succeeded.");
            }
            catch (Exception e)
            {
                Log.LogCritical($"{nameof(StartupBase)}.{nameof(Configure)} failed. The application {TechnicalName} needs to be restarted.: {e.Message}", e);
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
        protected virtual void ConfigureServicesInitialUrgentPart(IServiceCollection services)
        {
            var configurationSection = Configuration.GetSection("FulcrumApplication");
            FulcrumAssert.IsNotNull(configurationSection);
            Application.FulcrumApplicationHelper.WebBasicSetup(configurationSection);
            TechnicalName = FulcrumApplication.AppSettings.GetString("TechnicalName", true);
            FulcrumApplication.Setup.SynchronousFastLogger = GetSynchronousFastLogger();
        }

        /// <summary>
        /// Get the logger that should be use for logging for this application. The logger must implement <see cref="ISyncLogger"/>.
        /// </summary>
        /// <remarks>
        /// If you have a logger that implements <see cref="IAsyncLogger"/> you can make it an <see cref="ISyncLogger"/> by adding .AsSyncLogger after your instance.
        /// </remarks>
        protected abstract ISyncLogger GetSynchronousFastLogger();

        /// <summary>
        /// Configure Swagger
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        protected virtual void ConfigureServicesSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(ApiVersion, new Info {Title = FulcrumApplication.Setup.Name, Version = ApiVersion});
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

                var commentsFilePath = GetCommentsFilePath();
                if (commentsFilePath != null) c.IncludeXmlComments(commentsFilePath);
            });
        }


        /// <summary>
        /// The name of the XML file. Created based on the <see cref="TechnicalName"/>.
        /// </summary>
        /// <returns>The path or NULL if no file is found at that path.</returns>
        protected virtual string GetCommentsFilePath()
        {
            var xmlFile = $"{TechnicalName}.Service.xml";
            var path = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(path)) return path;

            Log.LogWarning($"Could not find API documentation file ({path})");
            return null;
        }

        /// <summary>
        /// Convenience method to catch dependency injection exceptions and log them as critical errors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        /// <param name="dependencyInjection"></param>
        /// <returns></returns>
        protected virtual T ValidateDependencyInjection<T>(IServiceProvider provider,
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

        /// <summary>
        /// This is where the application injects its own services.
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        /// <remarks>Always override this to inject your services.</remarks>
        protected abstract void DependencyInjectServices(IServiceCollection services);
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
        protected virtual void ConfigureAppMiddleware(IApplicationBuilder app, IHostingEnvironment env)
        {
            ConfigureSwaggerMiddleware(app, env);

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
                c.DocumentTitle = FulcrumApplication.Setup.Name;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{FulcrumApplication.Setup.Name} {ApiVersion}");
            });
        }
        #endregion

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(TechnicalName, nameof(TechnicalName), errorLocation);
            FulcrumValidate.IsNotNull(ApiVersion, nameof(TechnicalName), ApiVersion);
        }
    }
}
#endif