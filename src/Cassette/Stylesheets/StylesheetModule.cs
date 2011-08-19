﻿using System;
using System.Linq;
using System.Web;
using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class StylesheetModule : Module
    {
        public StylesheetModule(string directory)
            : base(directory)
        {
            ContentType = "text/css";
            Processor = new StylesheetPipeline();
        }

        protected static readonly string linkHtml = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
        protected static readonly string linkHtmlWithMedia = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" media=\"{1}\"/>";

        public string Media { get; set; }
        public IModuleProcessor<StylesheetModule> Processor { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                var url = application.UrlGenerator.CreateModuleUrl(this);
                if (string.IsNullOrEmpty(Media))
                {
                    return new HtmlString(string.Format(linkHtml, url));
                }
                else
                {
                    return new HtmlString(string.Format(linkHtmlWithMedia, url, Media));
                }
            }
            else
            {
                var hasMedia = string.IsNullOrEmpty(Media);
                var scripts = string.Join(Environment.NewLine,
                    from asset in Assets
                    let url = IsCompiledAsset(asset)
                        ? application.UrlGenerator.CreateAssetCompileUrl(this, asset)
                        : application.UrlGenerator.CreateAssetUrl(this, asset)
                    select hasMedia ? string.Format(linkHtml, url) 
                                    : string.Format(linkHtmlWithMedia, url, Media)
                );
                return new HtmlString(scripts);
            }
        }
    }
}