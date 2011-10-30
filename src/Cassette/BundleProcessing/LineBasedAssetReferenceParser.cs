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

using System.IO;
using System.Text.RegularExpressions;

namespace Cassette.BundleProcessing
{
    public class LineBasedAssetReferenceParser<T> : ProcessAssetsThatMatchFileExtension<T>
        where T : Bundle
    {
        protected LineBasedAssetReferenceParser(string fileExtension, Regex referenceRegex)
            : base(fileExtension)
        {
            this.referenceRegex = referenceRegex;
        }

        readonly Regex referenceRegex;

        protected override void Process(IAsset asset, Bundle bundle)
        {
            using (var reader = new StreamReader(asset.OpenStream()))
            {
                var lineNumber = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var match = referenceRegex.Match(line);
                    if (match.Success)
                    {
                        asset.AddReference(match.Groups["path"].Value, lineNumber);
                    }
                }
            }
        }
    }
}

