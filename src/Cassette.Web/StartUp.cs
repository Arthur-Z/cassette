﻿using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivator.PostApplicationStartMethod(
    typeof(Cassette.Web.StartUp), 
    "PostApplicationStart"
)]
[assembly: WebActivator.ApplicationShutdownMethod(
    typeof(Cassette.Web.StartUp),
    "ApplicationShutdown"
)]

namespace Cassette.Web
{
    public static class StartUp
    {
        static IsolatedStorageFile storage;
        public static CassetteApplication CassetteApplication { get; private set; }

        // If using an IoC container, this delegate can be replaced (in Application_Start) to
        // provide an alternative way to create the configuration object.
        public static Func<ICassetteConfiguration> CreateConfiguration = CreateConfigurationByScanningAssembliesForType;

        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            storage = IsolatedStorageFile.GetMachineStoreForAssembly();

            var configuration = CreateConfiguration();
            CassetteApplication = CreateCassetteApplication(configuration, storage);
            CassetteApplication.InitializeModuleContainers();
            CassetteApplication.InstallRoutes(RouteTable.Routes);
            
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }

        public static void ApplicationShutdown()
        {
            if (storage != null)
            {
                storage.Dispose();
                storage = null;
            }
        }

        static ICassetteConfiguration CreateConfigurationByScanningAssembliesForType()
        {
            // Scan all assemblies for implementation of the interface and create instance.
            var types = from filename in Directory.GetFiles(HttpRuntime.BinDirectory, "*.dll")
                        let assembly = Assembly.LoadFrom(filename)
                        from type in assembly.GetExportedTypes()
                        where type.IsClass
                           && !type.IsAbstract
                           && typeof(ICassetteConfiguration).IsAssignableFrom(type)
                        select type;

            var configType = types.FirstOrDefault();
            if (configType == null)
            {
                // No configuration defined. Any attempt to get asset modules later will fail with 
                // an exception. The exception message will tell the developer to create a configuration class.
                return new EmptyCassetteConfiguration();
            }
            else
            {
                return (ICassetteConfiguration)Activator.CreateInstance(configType);
            }
        }

        static CassetteApplication CreateCassetteApplication(ICassetteConfiguration configuration, IsolatedStorageFile storage)
        {
            var application = new CassetteApplication(
                new FileSystem(HttpRuntime.AppDomainAppPath),
                new UrlGenerator(HttpRuntime.AppDomainAppVirtualPath),
                new IsolatedStorageFileSystem(storage),
                GetDebugModeFromConfig() == false
            );
            configuration.Configure(application);
            return application;
        }

        static bool GetDebugModeFromConfig()
        {
            var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            return (compilation != null && compilation.Debug) || false;
        }
    }
}
