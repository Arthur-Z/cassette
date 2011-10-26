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

using Cassette.Configuration;
using Cassette.Stylesheets;
using Cassette.Scripts;

namespace Website
{
    public class CassetteConfiguration : ICassetteConfiguration
    {
        public void Configure(BundleCollection bundles, CassetteSettings settings)
        {
            bundles.Add(new StylesheetBundle("assets/styles"));
            bundles.Add(new ExternalScriptBundle("//ajax.googleapis.com/ajax/libs/jquery/1.6.3/jquery.min.js", "jquery"));
            bundles.AddForEachSubDirectory<ScriptBundle>("assets/scripts");        }
    }
}