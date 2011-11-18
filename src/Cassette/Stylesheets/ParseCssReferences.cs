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

using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ParseCssReferences : ParseReferences<StylesheetBundle>
    {
        protected override bool ShouldParseAsset(IAsset asset)
        {
            return asset.SourceFile.FullPath.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
        }

        internal override ICommentParser CreateCommentParser()
        {
            return new CssCommentParser();
        }
    }
}