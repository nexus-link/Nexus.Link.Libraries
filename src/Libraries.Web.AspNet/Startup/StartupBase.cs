
using Nexus.Link.Libraries.Core.Misc;
#if NETCOREAPP
using System;
using System.Reflection;
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
using Microsoft.AspNetCore.Mvc.Authorization;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Services.Contracts.Capabilities;
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
            FulcrumApplication.AppSettings = new AppSettings(Configuration.GetSection("FulcrumApplication"));
            IsBusinessApi = isBusinessApi;
        }

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
        /// Always override <see cref="DependencyInjectServices(IServiceCollection)"/> to inject your own services.
        /// Override <see cref="ConfigureServicesInitialUrgentPart"/> if you have things that needs to be initialized early.
        /// Override <see cref="ConfigureServicesSwagger"/> if you want to change how swagger is set up.
        /// </remarks>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            try
            {
                FulcrumApplication.Validate();
                ConfigureServicesInitialUrgentPart(services);
                FulcrumApplication.ValidateButNotInProduction();
                InternalContract.RequireValidated(this, GetType().FullName);
                var valueTranslatorFilter = new ValueTranslatorFilter(() => FulcrumApplication.Context.ClientPrincipal.Identity.Name);
                var mvc = services.AddMvc(opts =>
                {
                    opts.Filters.Add(valueTranslatorFilter);
                    if (!FulcrumApplication.IsInDevelopment) return;
                    Log.LogWarning($"Anonymous service usage is allowed, due to development mode.");
                    opts.Filters.Add(new AllowAnonymousFilter());
                });
                mvc
                    .SetCompatibilityVersion(CompatibilityVersion)
                    .ConfigureApplicationPartManager(apm =>
                        apm.FeatureProviders.Add(new RemoveRedundantControllers(_controllersToKeep)));
                ConfigureServicesSwagger(services);
                DependencyInjectServices(services);
                using (var serviceScope = services.BuildServiceProvider().CreateScope())
                {
                    var serviceProvider = serviceScope.ServiceProvider;
                    // TODO: Send the serviceProvider instead of the mvc
                    DependencyInjectServicesAdvanced(services, mvc);
                    ValueTranslatorFilter.TranslatorService = serviceProvider.GetService<ITranslatorService>();
                    // TODO: Send the serviceProvider instead of the mvc
                    AddControllersToMvc(services, mvc);
                }

                Log.LogInformation($"{nameof(StartupBase)}.{nameof(ConfigureServices)} succeeded.");
            }
            catch (Exception e)
            {
                Log.LogCritical(
                    $"{nameof(StartupBase)}.{nameof(ConfigureServices)} failed. The application {FulcrumApplication.Setup?.Name} needs to be restarted.: {e.Message}",
                    e);
                throw;
            }
        }

        private readonly IDictionary<Type, IEnumerable<Type>> _capabilityInterfaceToControllerClasses = new Dictionary<Type, IEnumerable<Type>>();
        private readonly HashSet<Type> _controllersToKeep = new HashSet<Type>();

        // https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/app-parts?view=aspnetcore-2.2
        // When we need a controller, this code will add all controllers in the same assembly.
        // See RemoveRedundantControllers for removing the controllers that are redundant.
        private void AddControllersToMvc(IServiceCollection services, IMvcBuilder mvcBuilder)
        {
            using (var serviceScope = services.BuildServiceProvider().CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                var assemblies = new HashSet<Assembly>();

                foreach (var serviceType in _capabilityInterfaceToControllerClasses.Keys)
                {
                    FulcrumAssert.IsTrue(serviceType.IsInterface, CodeLocation.AsString());
                    var service = serviceProvider.GetService(serviceType);
                    if (service == null) continue;
                    var controllerTypes = _capabilityInterfaceToControllerClasses[serviceType];
                    FulcrumAssert.IsNotNull(controllerTypes);        
                    var controllerList = controllerTypes as List<Type> ?? controllerTypes.ToList();

                    foreach (var controllerType in controllerList)
                    {
                        FulcrumAssert.IsTrue(controllerType.IsClass, CodeLocation.AsString());
                        var assembly = controllerType.GetTypeInfo().Assembly;
                        assemblies.Add(assembly);
                        _controllersToKeep.Add(controllerType);
                    }

                    if (controllerList.Count > 0)
                    {
                        var names = controllerList.Select(t => t.Name);
                        Log.LogInformation(
                            $"Injecting controllers for the capability {serviceType.Name}: {string.Join(", ", names)}");
                        Log.LogVerbose(
                            $"The capability {serviceType.Name} was implemented by {service.GetType().FullName}");
                    }
                }

                foreach (var assembly in assemblies)
                {
                    mvcBuilder.AddApplicationPart(assembly);
                    Log.LogInformation(
                        $"Injected all controllers in assembly {assembly.FullName}. The redundant ones will be removed later.");
                }
            }
        }

        /// <summary>
        /// Register which controllers that should be used for a specific capability interface.
        /// </summary>
        public void RegisterControllersForCapability<TControllerInjector>(params Type[] controllerTypes)
            where TControllerInjector : IControllerInjector
        {
            InternalContract.Require(typeof(TControllerInjector).IsInterface, 
                $"The type ({typeof(TControllerInjector).Name}) passed to {nameof(TControllerInjector)} must be an interface.");
            RegisterControllersForCapability(typeof(TControllerInjector), controllerTypes);
        }

        /// <summary>
        /// Register which controllers that should be used for a specific capability interface.
        /// </summary>
        public void RegisterControllersForCapability(Type capabilityInterface, params Type[] controllerTypes)
        {
            InternalContract.Require(capabilityInterface, type => type.IsInterface, nameof(capabilityInterface));
            InternalContract.Require(capabilityInterface.IsInterface, 
                $"The parameter {nameof(capabilityInterface)} must be an interface.");
            InternalContract.Require(typeof(IControllerInjector).IsAssignableFrom(capabilityInterface), 
                $"The parameter {nameof(capabilityInterface)} must inherit from {typeof(IControllerInjector).FullName}.");
            foreach (var controllerType in controllerTypes)
            {
                InternalContract.Require(controllerType, type => type.IsClass, nameof(controllerType));
            }

            _capabilityInterfaceToControllerClasses.Add(capabilityInterface, controllerTypes);
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

                app.UseHsts();
                app.UseHttpsRedirection();
                app.UseMvc();
                Log.LogInformation($"{nameof(StartupBase)}.{nameof(Configure)} succeeded.");
            }
            catch (Exception e)
            {
                Log.LogCritical(
                    $"{nameof(StartupBase)}.{nameof(Configure)} failed. The application {FulcrumApplication.Setup?.Name} needs to be restarted.: {e.Message}",
                    e);
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
                var commentsFilePath = GetCommentsFilePath();
                if (commentsFilePath != null) c.IncludeXmlComments(commentsFilePath);
                if (FulcrumApplication.IsInDevelopment) return;
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
            });
        }


        /// <summary>
        /// The name of the XML file. Created based on the TechnicalName.
        /// </summary>
        /// <returns>The path or NULL if no file is found at that path.</returns>
        protected virtual string GetCommentsFilePath()
        {
            var technicalName = FulcrumApplication.AppSettings.GetString("TechnicalName", true);
            var xmlFile = $"{technicalName}.Service.xml";
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
        /// <param name="mvc"></param>
        /// <remarks>Always override this to inject your services.</remarks>
        protected abstract void DependencyInjectServices(IServiceCollection services);

        /// <summary>
        /// This is where the application injects its own services.
        /// </summary>
        /// <param name="services">From the parameter to Startup.ConfigureServices.</param>
        /// <param name="mvc"></param>
        /// <remarks>Always override this to inject your services.</remarks>
        protected virtual void DependencyInjectServicesAdvanced(IServiceCollection services, IMvcBuilder mvcBuild)
        {

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
            var technicalName = FulcrumApplication.AppSettings.GetString("TechnicalName", true);
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.DocumentTitle = FulcrumApplication.Setup.Name;
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{technicalName} {ApiVersion}");
            });
        }

        #endregion

        /// <inheritdoc />
        public virtual void Validate(string errorLocation, string propertyPath = "")
        {
            FulcrumValidate.IsNotNull(ApiVersion, nameof(ApiVersion), errorLocation);
        }
    }
}
#endif