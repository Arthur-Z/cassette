﻿using System;
using System.Collections.Generic;
using System.Web;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetModule : StylesheetModule, IModuleSource<StylesheetModule>, IExternalModule
    {
        public ExternalStylesheetModule(string url)
            : base(url)
        {
            this.url = url;
        }

        public ExternalStylesheetModule(string name, string url) 
            : base(PathUtilities.AppRelative(name))
        {
            this.url = url;
        }

        readonly string url;

        public override void Process(ICassetteApplication application)
        {
            // No processing required.
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (string.IsNullOrEmpty(Media))
            {
                return new HtmlString(string.Format(LinkHtml, url));
            }
            else
            {
                return new HtmlString(string.Format(LinkHtmlWithMedia, url, Media));
            }
        }

        public override bool ContainsPath(string path)
        {
            return base.ContainsPath(path) || url.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        IEnumerable<StylesheetModule> IModuleSource<StylesheetModule>.GetModules(IModuleFactory<StylesheetModule> moduleFactory, ICassetteApplication application)
        {
            yield return this;
        }
    }
}
