﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.HtmlTemplates;
using Cassette.IO;
using Cassette.Manifests;
using Cassette.Scripts;
using Cassette.Stylesheets;
#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.Configuration
{
    /// <summary>
    /// Settings that control Cassette's behavior.
    /// </summary>
    public class CassetteSettings
    {
        readonly Dictionary<Type, object> bundleDefaultsByType = new Dictionary<Type, object>(); 
        readonly Lazy<ICassetteManifestCache> cassetteManifestCache;
        readonly List<Func<string, bool>> allowPathPredicates = new List<Func<string, bool>>();
 
        public CassetteSettings(string cacheVersion)
        {
            bundleDefaultsByType = new Dictionary<Type, object>
            {
                { typeof(ScriptBundle), new BundleDefaults<ScriptBundle>() },
                { typeof(StylesheetBundle), new BundleDefaults<StylesheetBundle>() },
                { typeof(HtmlTemplateBundle), new BundleDefaults<HtmlTemplateBundle>() },
            };

            Version = cacheVersion;

            cassetteManifestCache = new Lazy<ICassetteManifestCache>(
                () => new CassetteManifestCache(CacheDirectory.GetFile("cassette.xml"))
            );
        }

        /// <summary>
        /// When true, Cassette has already loaded bundles from a compile-time generated manifest file.
        /// The application's Cassette configuration MUST NOT add bundles to the bundle collection.
        /// </summary>
        public bool IsUsingPrecompiledManifest { get; internal set; }

        /// <summary>
        /// When this property is true, Cassette will output debug-friendly assets. When false, combined, minified bundles are used instead.
        /// </summary>
        public bool IsDebuggingEnabled { get; set; }

        /// <summary>
        /// When true (the default), Cassette will buffer page output and rewrite to allow bundle references to be inserted into &lt;head&gt;
        /// after it has already been rendered. Disable this when &lt;system.webServer&gt;/&lt;urlCompression dynamicCompressionBeforeCache="true"&gt;
        /// is in Web.config.
        /// </summary>
        public bool IsHtmlRewritingEnabled { get; set; }

        /// <summary>
        /// The directory containing the original bundle asset files.
        /// </summary>
        public IDirectory SourceDirectory { get; set; }

        /// <summary>
        /// The directory used to cache combined, minified bundles.
        /// </summary>
        public IDirectory CacheDirectory { get; set; }

        /// <summary>
        /// The <see cref="IUrlModifier"/> used to convert application relative URLs into absolute URLs.
        /// </summary>
        public IUrlModifier UrlModifier { get; set; }

        public IUrlGenerator UrlGenerator { get; set; }

        internal bool AllowRemoteDiagnostics { get; set; }

        internal string Version { get; private set; }

        internal ICassetteManifestCache CassetteManifestCache
        {
            get { return cassetteManifestCache.Value; }
        }

        public void ModifyDefaults<T>(Action<BundleDefaults<T>> modifications)
            where T : Bundle
        {
            var defaults = (BundleDefaults<T>)bundleDefaultsByType[typeof(T)];
            modifications(defaults);
        }

        public BundleDefaults<T> GetDefaults<T>()
            where T : Bundle
        {
            return (BundleDefaults<T>)bundleDefaultsByType[typeof(T)];
        }

        internal IBundleDefaults GetDefaults(Type bundleType)
        {
            return (IBundleDefaults)bundleDefaultsByType[bundleType];
        }

        internal IBundleContainerFactory GetBundleContainerFactory(IEnumerable<ICassetteConfiguration> cassetteConfigurations)
        {
            var bundles = ExecuteCassetteConfiguration(cassetteConfigurations);
            if (IsDebuggingEnabled)
            {
                return new BundleContainerFactory(bundles, this);
            }
            else
            {
                return new CachedBundleContainerFactory(bundles, CassetteManifestCache, this);
            }
        }

        BundleCollection ExecuteCassetteConfiguration(IEnumerable<ICassetteConfiguration> cassetteConfigurations)
        {
            var bundles = new BundleCollection(this);
            foreach (var configuration in cassetteConfigurations)
            {
                Trace.Source.TraceInformation("Executing configuration {0}", configuration.GetType().AssemblyQualifiedName);
                configuration.Configure(bundles, this);
            }
            return bundles;
        }

        internal bool CanRequestRawFile(string filePath)
        {
            return allowPathPredicates.Any(predicate => predicate(filePath));
        }

        public void AllowRawFileRequest(Func<string, bool> pathIsAllowed)
        {
            allowPathPredicates.Add(pathIsAllowed);
        }
    }
}