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

using Cassette.Utilities;

namespace Cassette.Scripts
{
    class ScriptBundleFactory : BundleFactoryBase<ScriptBundle>
    {
        public override ScriptBundle CreateBundle(string pathOrUrl)
        {
            if (pathOrUrl.IsUrl())
            {
                return new ExternalScriptBundle(pathOrUrl);
            }
            else
            {
                return new ScriptBundle(pathOrUrl);
            }
        }

        protected override ScriptBundle CreateBundleCore(string path, BundleDescriptor bundleDescriptor)
        {
            if (bundleDescriptor.ExternalUrl != null)
            {
                return new ExternalScriptBundle(
                    bundleDescriptor.ExternalUrl,
                    path,
                    bundleDescriptor.FallbackCondition
                );
            }
            else
            {
                return new ScriptBundle(path);
            }
        }
    }
}