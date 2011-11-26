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

using System.Text;

namespace Cassette.Stylesheets
{
    class StylesheetHtmlRenderer : IBundleHtmlRenderer<StylesheetBundle>
    {
        public StylesheetHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(StylesheetBundle bundle)
        {
            var url = urlGenerator.CreateBundleUrl(bundle);
            var html = new StringBuilder();
            var hasCondition = !string.IsNullOrEmpty(bundle.Condition);
            if (hasCondition)
            {
                html.AppendLine("<!--[if " + bundle.Condition + "]>");
            }
            if (string.IsNullOrEmpty(bundle.Media))
            {
                html.AppendFormat(HtmlConstants.LinkHtml, url);
            }
            else
            {
                html.AppendFormat(HtmlConstants.LinkWithMediaHtml, url, bundle.Media);
            }
            if (hasCondition)
            {
                html.AppendLine();
                html.Append("<![endif]-->");
            }
            return html.ToString();
        }
    }
}