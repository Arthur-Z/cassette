﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class ParseCssReferences : ModuleProcessorOfAssetsMatchingFileExtension<StylesheetModule>
    {
        public ParseCssReferences()
            : base("css")
        {
        }

        static readonly Regex CssCommentRegex = new Regex(
            @"/\*(?<body>.*?)\*/",
            RegexOptions.Singleline
        );
        static readonly Regex ReferenceRegex = new Regex(
            @"@reference \s+ (?<quote>[""']) (?<path>.*?) \<quote> \s* ;?",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );

        protected override void Process(IAsset asset, Module module)
        {
            var css = ReadAllCss(asset);
            foreach (var reference in ParseReferences(css))
            {
                // TODO: Add line number tracking to the parser.
                // For now use -1 as dummy line number.
                asset.AddReference(reference, -1);
            }
        }

        string ReadAllCss(IAsset asset)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                return reader.ReadToEnd();
            }
        }

        IEnumerable<string> ParseReferences(string css)
        {
            var commentBodies = CssCommentRegex
                    .Matches(css)
                    .Cast<Match>()
                    .Select(match => match.Groups["body"].Value);

            return from body in commentBodies
                   from match in ReferenceRegex.Matches(body).Cast<Match>()
                   where match.Groups["path"].Success
                   select match.Groups["path"].Value;
        }
    }
}
