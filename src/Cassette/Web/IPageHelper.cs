﻿using System;
using System.Web;

namespace Cassette.Web
{
    public interface IPageHelper
    {
        void ReferenceScript(string scriptPathOrUrl);
        void ReferenceExternalScript(string externalScriptUrl, string location);
        IHtmlString RenderScripts(string location);

        void ReferenceStylesheet(string stylesheetPath);
        IHtmlString RenderStylesheetLinks();

        string ReplacePlaceholders(string line);
    }
}
