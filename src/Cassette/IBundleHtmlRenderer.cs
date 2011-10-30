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

using System.Web;

namespace Cassette
{
    /// <summary>
    /// Creates the HTML required to reference a bundle in a page.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Bundle"/>.</typeparam>
    public interface IBundleHtmlRenderer<in T>
        where T : Bundle
    {
        /// <summary>
        /// Returns the HTML elements that will include the bundle in a page.
        /// </summary>
        /// <param name="bundle">The bundle to render.</param>
        /// <returns>The HTML elements that will include the bundle in a page.</returns>
        IHtmlString Render(T bundle);
    }
}