﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.UI;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivator.PreApplicationStartMethod(
    typeof(Cassette.Web.StartUp),
    "PreApplicationStart"
)]
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
        static ICassetteConfiguration configuration;
        static IsolatedStorageFile storage;
        static CassetteApplicationContainer<CassetteApplication> applicationContainer;
 
        public static CassetteApplication CassetteApplication
        {
            get { return applicationContainer.Application; }
        }

        // If using an IoC container, this delegate can be replaced (in Application_Start) to
        // provide an alternative way to create the configuration object.
        public static Func<ICassetteConfiguration> CreateConfiguration = CreateConfigurationByScanningAssembliesForType;

        public static void PreApplicationStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }

        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            storage = IsolatedStorageFile.GetMachineStoreForAssembly();
            
            configuration = CreateConfiguration();
            if (ShouldOptimizeOutput())
            {
                applicationContainer = new CassetteApplicationContainer<CassetteApplication>(CreateCassetteApplication);                
            }
            else
            {
                applicationContainer = new CassetteApplicationContainer<CassetteApplication>(CreateCassetteApplication, HttpRuntime.AppDomainAppPath);                
            }
            
            Assets.GetApplication = () => CassetteApplication;
        }

        public static void ApplicationShutdown()
        {
            storage.Dispose();
            applicationContainer.Dispose();
        }

        static ICassetteConfiguration CreateConfigurationByScanningAssembliesForType()
        {
            // Scan all assemblies for implementation of the interface and create instance.
            var types = from assembly in LoadAllAssemblies()
                        from type in assembly.GetExportedTypes()
                        where type.IsClass
                           && !type.IsAbstract
                           && type != typeof(EmptyCassetteConfiguration)
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

        static IEnumerable<Assembly> LoadAllAssemblies()
        {
            const int COR_E_ASSEMBLYEXPECTED = -2146234344;
            foreach (var filename in Directory.GetFiles(HttpRuntime.BinDirectory, "*.dll"))
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(filename);
                }
                catch (BadImageFormatException exception)
                {
                    if (Marshal.GetHRForException(exception) == COR_E_ASSEMBLYEXPECTED) // Was not a managed DLL.
                    {
                        continue;
                    }
                    throw;
                }
                yield return assembly;
            }
        }

        static CassetteApplication CreateCassetteApplication()
        {
            return new CassetteApplication(
                configuration,
                new FileSystem(HttpRuntime.AppDomainAppPath),
                GetCacheDirectory(),
                ShouldOptimizeOutput(),
                GetConfigurationVersion(HttpRuntime.AppDomainAppVirtualPath),
                new UrlGenerator(HttpRuntime.AppDomainAppVirtualPath),
                RouteTable.Routes,
                GetCurrentHttpContext
            );
        }

        static IFileSystem GetCacheDirectory()
        {
            return new IsolatedStorageFileSystem(storage);
            // TODO: Add configuration setting to use App_Data
            //return new FileSystem(Path.Combine(HttpRuntime.AppDomainAppPath, "App_Data", ".CassetteCache"));
        }

        static bool ShouldOptimizeOutput()
        {
            var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            return (compilation != null && compilation.Debug) == false;
        }

        static string GetConfigurationVersion(string virtualDirectory)
        {
            var assemblyVersion = configuration.GetType()
                .Assembly
                .GetName()
                .Version
                .ToString();
            return assemblyVersion + "|" + virtualDirectory;
        }

        static HttpContextBase GetCurrentHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}
