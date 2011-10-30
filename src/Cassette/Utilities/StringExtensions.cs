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
using System.IO;

namespace Cassette.Utilities
{
    /// <summary>
    /// Utility methods for strings.
    /// </summary>
    static class StringExtensions
    {
        /// <summary>
        /// Returns a new stream containing the contents of the string, using UTF-8 encoding.
        /// The stream's Position property is set to zero.
        /// </summary>
        /// <param name="s">The string to convert into a stream.</param>
        /// <returns>A new stream.</returns>
        public static Stream AsStream(this string s)
        {
            var source = new MemoryStream();
            var writer = new StreamWriter(source);
            writer.Write(s);
            writer.Flush();
            source.Position = 0;
            return source;
        }

        public static bool IsUrl(this string s)
        {
            return s.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                || s.StartsWith("//");
        }
    }
}