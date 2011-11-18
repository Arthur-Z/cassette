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

using System.Linq;
using System.Collections.Generic;

namespace Cassette
{
    public class DirectorySource<T> : FileSystemBundleSource<T>
        where T : Bundle
    {
        public DirectorySource(params string[] relativeDirectoryPaths)
        {
            this.relativeDirectoryPaths = relativeDirectoryPaths
                .Select(EnsureApplicationRelativePath)
                .ToArray();
        }

        readonly string[] relativeDirectoryPaths;

        protected override IEnumerable<string> GetBundleDirectoryPaths(ICassetteApplication application)
        {
            return relativeDirectoryPaths;
        }
    }
}

