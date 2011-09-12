﻿using System;
using System.Collections.Generic;
using System.Web;
using Cassette.Utilities;

namespace Cassette.Scripts
{
    public class ExternalScriptModule : ScriptModule, IModuleSource<ScriptModule>, IExternalModule
    {
        public ExternalScriptModule(string url)
            : this(url, url)
        {
        }

        public ExternalScriptModule(string name, string url)
            : base(PathUtilities.AppRelative(name))
        {
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");

            this.url = url;
        }

        public ExternalScriptModule(string name, string url, string javaScriptFallbackCondition, string fallbackUrl)
            : base(PathUtilities.AppRelative(name))
        {
            if (url == null) throw new ArgumentNullException("url");
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL is required.", "url");
            if (javaScriptFallbackCondition == null) throw new ArgumentNullException("javaScriptFallbackCondition");
            if (string.IsNullOrWhiteSpace(javaScriptFallbackCondition)) throw new ArgumentException("JavaScript condition is required.", "javaScriptFallbackCondition");
            if (fallbackUrl == null) throw new ArgumentNullException("fallbackUrl");
            if (string.IsNullOrWhiteSpace(fallbackUrl)) throw new ArgumentException("Fallback URL is required.", "fallbackUrl");
            
            this.url = url;
            this.javaScriptFallbackCondition = javaScriptFallbackCondition;
            this.fallbackUrl = PathUtilities.AppRelative(fallbackUrl);
        }

        readonly string url;
        string javaScriptFallbackCondition;
        readonly string fallbackUrl;

        public string Url
        {
            get { return url; }
        }

        public string FallbackCondition
        {
            get { return javaScriptFallbackCondition; }
        }

        public void AddFallback(string javaScriptFallbackCondition, IEnumerable<IAsset> fallbackAssets)
        {
            this.javaScriptFallbackCondition = javaScriptFallbackCondition;
            AddAssets(fallbackAssets, true);
        }

        public override void Process(ICassetteApplication application)
        {
            if (string.IsNullOrEmpty(fallbackUrl) == false)
            {
                Assets.Add(new Asset(fallbackUrl, this, application.RootDirectory.GetFile(fallbackUrl.Substring(2))));
            }
            base.Process(application);
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            var externalRenderer = new ExternalScriptModuleHtmlRenderer(Renderer, application);
            return externalRenderer.Render(this);
        }

        public override bool ContainsPath(string path)
        {
            return base.ContainsPath(path) || url.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        IEnumerable<ScriptModule> IModuleSource<ScriptModule>.GetModules(IModuleFactory<ScriptModule> moduleFactory, ICassetteApplication application)
        {
            yield return this;
        }
    }
}