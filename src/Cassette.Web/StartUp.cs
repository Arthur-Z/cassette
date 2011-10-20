﻿#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.Configuration;
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
        static IEnumerable<ICassetteConfiguration> configurations;
        static IsolatedStorageFile storage;
        static CassetteApplicationContainer<CassetteApplication> applicationContainer;
 
        internal static CassetteApplication CassetteApplication
        {
            get { return applicationContainer.Application; }
        }

        // If using an IoC container, this delegate can be replaced (in Application_Start) to
        // provide an alternative way to create the configuration object.
        public static Func<IEnumerable<ICassetteConfiguration>> CreateConfigurations = CreateConfigurationsByScanningAssembliesForType;

        public static void PreApplicationStart()
        {
            Trace.Source.TraceInformation("Registering CassetteHttpModule.");
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }

        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            storage = IsolatedStorageFile.GetMachineStoreForAssembly(); // TODO: Check if this should be GetMachineStoreForApplication instead
            
            configurations = CreateConfigurations();
            applicationContainer = GetSystemWebCompilationDebug() 
                ? new CassetteApplicationContainer<CassetteApplication>(CreateCassetteApplication, HttpRuntime.AppDomainAppPath)
                : new CassetteApplicationContainer<CassetteApplication>(CreateCassetteApplication);
            
            Assets.GetApplication = () => CassetteApplication;
        }

        public static void ApplicationShutdown()
        {
            Trace.Source.TraceInformation("Application shutdown - disposing resources.");
            storage.Dispose();
            applicationContainer.Dispose();
        }

        static IEnumerable<ICassetteConfiguration> CreateConfigurationsByScanningAssembliesForType()
        {
            Trace.Source.TraceInformation("Creating CassetteConfigurations by scanning assemblies.");
            // Scan all assemblies for implementations of the interface and create instances.
            return from assembly in BuildManager.GetReferencedAssemblies().Cast<Assembly>()
                   from type in GetConfigurationTypes(assembly)
                   select CreateConfigurationInstance(type);
        }

        static IEnumerable<Type> GetConfigurationTypes(Assembly assembly)
        {
            IEnumerable<Type> types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                // Some types failed to load, often due to a referenced assembly being missing.
                // This is not usually a problem, so just continue with whatever did load.
                types = exception.Types.Where(type => type != null);
            }
            return types.Where(IsCassetteConfigurationType);
        } 

        static bool IsCassetteConfigurationType(Type type)
        {
            return type.IsPublic
                   && type.IsClass
                   && !type.IsAbstract
                   && typeof(ICassetteConfiguration).IsAssignableFrom(type);
        }

        static ICassetteConfiguration CreateConfigurationInstance(Type type)
        {
            Trace.Source.TraceInformation("Creating {0}.", type.FullName);

            return (ICassetteConfiguration)Activator.CreateInstance(type);
        }

        static CassetteApplication CreateCassetteApplication()
        {
            var allConfigurations = GetAllConfigurations();
            var settings = new CassetteSettings
            {
                CacheVersion = GetConfigurationVersion(allConfigurations, HttpRuntime.AppDomainAppVirtualPath)
            };
            var bundles = new BundleCollection(settings);

            foreach (var configuration in allConfigurations)
            {
                Trace.Source.TraceInformation("Executing configuration {0}", configuration.GetType().AssemblyQualifiedName);
                configuration.Configure(bundles, settings);
            }
            
            var routing = new CassetteRouting(settings.UrlModifier);

            Trace.Source.TraceInformation("Creating Cassette application object");
            Trace.Source.TraceInformation("IsDebuggingEnabled: {0}", settings.IsDebuggingEnabled);
            Trace.Source.TraceInformation("Cache version: {0}", settings.CacheVersion);

            return new CassetteApplication(
                bundles,
                settings,
                routing,
                RouteTable.Routes,
                GetCurrentHttpContext
            );
        }

        static List<ICassetteConfiguration> GetAllConfigurations()
        {
            var sourceDirectory = HttpRuntime.AppDomainAppPath;
            var isDebuggingEnabled = GetSystemWebCompilationDebug();
            var initialConfiguration = new InitialConfiguration(sourceDirectory, storage, isDebuggingEnabled);

            var allConfigurations = new List<ICassetteConfiguration> { initialConfiguration };
            allConfigurations.AddRange(configurations);
            return allConfigurations;
        }

        static bool GetSystemWebCompilationDebug()
        {
            var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            return compilation != null && compilation.Debug;
        }

        static string GetConfigurationVersion(IEnumerable<ICassetteConfiguration> configurations, string virtualDirectory)
        {
            var assemblyVersion = configurations.Select(
                configuration => new AssemblyName(configuration.GetType().Assembly.FullName).Version.ToString()
            ).Distinct();

            var parts = assemblyVersion.Concat(new[] { virtualDirectory });
            return string.Join("|", parts);
        }

        static HttpContextBase GetCurrentHttpContext()
        {
            return new HttpContextWrapper(HttpContext.Current);
        }
    }
}

