﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Configuration;
using System.Web.Routing;
using Cassette.Configuration;
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
    /// <summary>
    /// Controls the lifetime of the Cassette infrastructure by handling application startup and shutdown.
    /// </summary>
    public static class StartUp
    {
        static CassetteApplicationContainer<CassetteApplication> _container;
        static Stopwatch _startupTimer;
        /// <summary>
        /// Collects all of Cassette's trace output during start-up.
        /// </summary>
        static readonly TraceListener StartupTraceListener = new StringBuilderTraceListener
        {
            TraceOutputOptions = TraceOptions.DateTime,
            Filter = new EventTypeFilter(SourceLevels.All)
        };

        // ReSharper disable UnusedMember.Global
        public static void PreApplicationStart()
        {
            _startupTimer = Stopwatch.StartNew();
            Trace.Source.Listeners.Add(StartupTraceListener);
            Trace.Source.TraceInformation("Registering CassetteHttpModule.");
            DynamicModuleUtility.RegisterModule(typeof(CassetteHttpModule));
        }
        // ReSharper restore UnusedMember.Global

        // ReSharper disable UnusedMember.Global
        // This runs *after* Global.asax Application_Start.
        public static void PostApplicationStart()
        {
            Trace.Source.TraceInformation("PostApplicationStart.");
            try
            {
                InitializeApplicationContainer();
                InstallRoutes();
                Trace.Source.TraceInformation("Cassette startup completed. It took {0} ms.", _startupTimer.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                Trace.Source.TraceEvent(TraceEventType.Error, 0, ex.Message);
                throw;
            }
            finally
            {
                if (_startupTimer != null) _startupTimer.Stop();
                Trace.Source.Flush();
                Trace.Source.Listeners.Remove(StartupTraceListener);
            }
        }
        // ReSharper restore UnusedMember.Global

        static void InitializeApplicationContainer()
        {
            var factory = new CassetteApplicationContainerFactory(
                CassetteConfigurationFactory(),
                GetCassetteConfigurationSection(),
                IsAspNetDebugging()
            );
            _container = factory.CreateContainer();
            CassetteApplicationContainer.SetContainerSingleton(_container);
            _container.ForceApplicationCreation();
        }

        static void InstallRoutes()
        {
            var routing = new CassetteRouting(_container, RoutingHelpers.RoutePrefix);
            routing.InstallRoutes(RouteTable.Routes);
        }

        // ReSharper disable UnusedMember.Global
        public static void ApplicationShutdown()
        {
            Trace.Source.TraceInformation("Application shutdown - disposing resources.");
            _container.Dispose();
        }
        // ReSharper restore UnusedMember.Global

        // ReSharper disable MemberCanBePrivate.Global
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        /// <summary>
        /// Set this optional property to provide a custom function that creates the application's Cassette configuration objects.
        /// Assignment to this property should happen in Application_Start.
        /// If not set, Cassette's default assembly scanner will look for configuration types to create.
        /// </summary>
        public static Func<IEnumerable<ICassetteConfiguration>> CreateConfigurations { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore MemberCanBePrivate.Global

        static internal string TraceOutput
        {
            get { return StartupTraceListener.ToString(); }
        }

        static CassetteConfigurationSection GetCassetteConfigurationSection()
        {
            return (WebConfigurationManager.GetSection("cassette") as CassetteConfigurationSection) 
                   ?? new CassetteConfigurationSection();
        }

        static bool IsAspNetDebugging()
        {
            var compilation = WebConfigurationManager.GetSection("system.web/compilation") as CompilationSection;
            return compilation != null && compilation.Debug;
        }

        static ICassetteConfigurationFactory CassetteConfigurationFactory()
        {
            if (CreateConfigurations == null)
            {
                return new AssemblyScanningCassetteConfigurationFactory(GetApplicationAssemblies());
            }
            else
            {
                return new DelegateCassetteConfigurationFactory(CreateConfigurations);
            }
        }

        static IEnumerable<Assembly> GetApplicationAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().Cast<Assembly>();
        }
    }
}