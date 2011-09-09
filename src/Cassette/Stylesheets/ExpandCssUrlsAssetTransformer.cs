﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrlsAssetTransformer : IAssetTransformer
    {
        public ExpandCssUrlsAssetTransformer(ICassetteApplication application)
        {
            this.application = application;
        }

        readonly ICassetteApplication application;

        static readonly Regex CssUrlRegex = new Regex(
            @"\b url \s* \( \s* (?<url>.*?) \s* \)", 
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );
        static readonly Regex AbsoluteUrlRegex = new Regex(
            @"^(https?:|data:|//)"
        );

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var css = ReadCss(openSourceStream);
                var currentDirectory = GetCurrentDirectory(asset);
                var urlMatches = UrlMatchesInReverse(css);
                var builder = new StringBuilder(css);
                foreach (var match in urlMatches)
                {
                    var matchedUrlGroup = match.Groups["url"];
                    var relativeFilename = GetImageFilename(matchedUrlGroup, currentDirectory);
                    ExpandUrl(builder, matchedUrlGroup, relativeFilename);

                    asset.AddRawFileReference(relativeFilename);
                }
                return builder.ToString().AsStream();
            };
        }

        string ReadCss(Func<Stream> openSourceStream)
        {
            using (var reader = new StreamReader(openSourceStream()))
            {
                return reader.ReadToEnd();
            }
        }

        string GetCurrentDirectory(IAsset asset)
        {
            return Path.GetDirectoryName(asset.SourceFilename);
        }

        /// <remarks>
        /// Matches need to be in reverse because we'll be modifying the string.
        /// Working backwards means we won't disturb the match index values.
        /// </remarks>
        IEnumerable<Match> UrlMatchesInReverse(string css)
        {
            return CssUrlRegex
                .Matches(css)
                .Cast<Match>()
                .Where(match => AbsoluteUrlRegex.IsMatch(match.Groups["url"].Value) == false)
                .OrderByDescending(match => match.Index);
        }

        void ExpandUrl(StringBuilder builder, Group matchedUrlGroup, string relativeFilename)
        {
            relativeFilename = RemoveFragment(relativeFilename);
            var hash = HashFileContents(relativeFilename);
            var absoluteUrl = application.UrlGenerator.CreateImageUrl(relativeFilename, hash);
            builder.Remove(matchedUrlGroup.Index, matchedUrlGroup.Length);
            builder.Insert(matchedUrlGroup.Index, absoluteUrl);
        }

        string RemoveFragment(string relativeFilename)
        {
            var index = relativeFilename.IndexOf('#');
            if (index < 0) return relativeFilename;
            return relativeFilename.Substring(0, index);
        }

        string HashFileContents(string applicationRelativeFilename)
        {
            var file = application.RootDirectory.GetFile(applicationRelativeFilename.Substring(2));
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read))
            {
                return fileStream.ComputeSHA1Hash().ToHexString();
            }
        }

        string GetImageFilename(Group matchedUrlGroup, string currentDirectory)
        {
            var originalUrl = matchedUrlGroup.Value.Trim('"', '\'');
            return PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(currentDirectory, originalUrl));
        }
    }
}
