﻿using System;
using System.Text.RegularExpressions;

namespace Cassette.Scripts
{
#pragma warning disable 659
    class InlineScriptBundle : ScriptBundle
    {
        readonly string scriptContent;
        readonly bool isContentScriptTag;

        public InlineScriptBundle(string scriptContent)
        {
            this.scriptContent = scriptContent;
            isContentScriptTag = scriptContent != null &&
                                 DetectScriptRegex.IsMatch(scriptContent, 0);
            if (isContentScriptTag)
            {
                HtmlAttributes.Clear();
            }
        }

        protected override void ProcessCore(CassetteSettings settings)
        {
        }

        static readonly Regex DetectScriptRegex = new Regex(
            @"\A \s* <script \b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace
        );

        /// <summary>
        /// Handle cases of the content already wrapped in a &lt;script&gt; tag.
        /// </summary>
        /// <returns></returns>
        string GetScriptContent()
        {
            var htmlAttributes = HtmlAttributes.CombinedAttributes; // should start with a space

            if (isContentScriptTag)
            {
                return DetectScriptRegex.Replace(scriptContent,
                    m => m.Value + htmlAttributes, 1, 0); // don't need a space after the attributes - the regex is checking for "\b"
            }
            return String.Format(
                HtmlConstants.InlineScriptHtml,
                htmlAttributes,
                Environment.NewLine,
                scriptContent
                );
        }

        internal override string Render()
        {
            var content = GetScriptContent();
            var conditionalRenderer = new ConditionalRenderer();
            return conditionalRenderer.Render(Condition, html => html.Append(content));
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(obj, this);
        }
    }
#pragma warning restore 659
}