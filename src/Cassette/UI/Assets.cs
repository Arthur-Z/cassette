﻿using System;
using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace Cassette.UI
{
    public static class Assets
    {
        public static Func<ICassetteApplication> GetApplication;

        public static IPageAssetManager<ScriptModule> Scripts
        {
            get { return Application.GetPageAssetManager<ScriptModule>(); }
        }

        public static IPageAssetManager<StylesheetModule> Stylesheets
        {
            get { return Application.GetPageAssetManager<StylesheetModule>(); }
        }

        public static IPageAssetManager<HtmlTemplateModule> HtmlTemplates
        {
            get { return Application.GetPageAssetManager<HtmlTemplateModule>(); }
        }

        static ICassetteApplication Application
        {
            get
            {
                if (GetApplication == null)
                {
                    // We rely on Cassette.Web (or some other) integration library to hook up its application object.
                    // If the delegate is null then the developer probably forgot to reference the integration library.
                    throw new InvalidOperationException("A Cassette application has not been assigned. Make sure a Cassette integration library has been referenced. For example, reference Cassette.Web.dll");
                }
                return GetApplication();
            }
        }
    }
}
