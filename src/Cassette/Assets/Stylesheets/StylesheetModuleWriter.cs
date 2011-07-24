﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Ajax.Utilities;
using Cassette.Less;

namespace Cassette.Assets.Stylesheets
{
    public class StylesheetModuleWriter : IModuleWriter
    {
        readonly TextWriter textWriter;
        readonly string rootDirectory;
        readonly string applicationRoot;
        readonly Func<string, string> readFileText;
        readonly ILessCompiler lessCompiler;
        readonly Regex cssUrlRegex = new Regex(@"\b url \s* \( \s* (?<url>.*?) \s* \)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        readonly Regex hasUriProtocolRegex = new Regex(@"^[^:/?#]+:");

        public StylesheetModuleWriter(TextWriter textWriter, string rootDirectory, string applicationRoot, Func<string, string> readFileText, ILessCompiler lessCompiler)
        {
            this.textWriter = textWriter;
            this.rootDirectory = rootDirectory;
            this.applicationRoot = applicationRoot;
            this.readFileText = readFileText;
            this.lessCompiler = lessCompiler;
        }

        public void Write(Module module)
        {
            var minifier = new Minifier();

            textWriter.Write(
                minifier.MinifyStyleSheet(
                    string.Join(
                        "\r\n",
                        module.Assets.Select(ReadStylesheet)
                    )
                )
            );
        }

        string ReadStylesheet(Asset asset)
        {
            if (asset.Path.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                return ReadCss(asset);
            }
            else // .less
            {
                return ReadLess(asset);
            }
        }

        string ReadCss(Asset asset)
        {
            var css = readFileText(rootDirectory + asset.Path);
            var currentDirectory = applicationRoot + asset.Path.Substring(0, asset.Path.LastIndexOf('/') + 1);
            css = ExpandRelativeUrls(css, currentDirectory);
            return css;
        }

        string ReadLess(Asset asset)
        {
            return lessCompiler.CompileFile(rootDirectory + asset.Path);
        }

        string ExpandRelativeUrls(string css, string currentDirectory)
        {
            // Need to work backwards so the indexes of URLs don't get offset when we insert new text.
            var matchesInReverseOrder = cssUrlRegex
                .Matches(css)
                .Cast<Match>()
                .OrderByDescending(m => m.Index);

            var builder = new StringBuilder(css);
            foreach (var match in matchesInReverseOrder)
            {
                var matchedUrlGroup = match.Groups["url"];
                ReplaceUrlWithAbsoluteUrl(builder, matchedUrlGroup, currentDirectory);
            }
            return builder.ToString();
        }

        void ReplaceUrlWithAbsoluteUrl(StringBuilder builder, Group matchedUrlGroup, string currentDirectory)
        {
            var relativeUrl = matchedUrlGroup.Value.Trim('"', '\'');
            var absoluteUrl = CreateAbsoluteUrl(relativeUrl, currentDirectory);
            builder.Remove(matchedUrlGroup.Index, matchedUrlGroup.Length);
            builder.Insert(matchedUrlGroup.Index, absoluteUrl);
        }

        string CreateAbsoluteUrl(string url, string currentDirectory)
        {
            if (url.StartsWith("/")) return url;
            if (hasUriProtocolRegex.IsMatch(url)) return url;
            return currentDirectory + url;
        }
    }
}
