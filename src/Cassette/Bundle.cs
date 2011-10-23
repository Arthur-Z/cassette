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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.BundleProcessing;
using Cassette.Utilities;

namespace Cassette
{
    [System.Diagnostics.DebuggerDisplay("{Path}")]
    public abstract class Bundle : IDisposable
    {
        readonly bool useDefaultBundleInitializer;
        readonly string path;
        readonly List<IBundleInitializer> bundleInitializers = new List<IBundleInitializer>();
        readonly List<IAsset> assets = new List<IAsset>();
        readonly HashSet<string> references = new HashSet<string>();
        bool hasSortedAssets;

        protected Bundle(string applicationRelativePath, bool useDefaultBundleInitializer)
        {
            if (applicationRelativePath == null) throw new ArgumentNullException("applicationRelativePath");
            path = PathUtilities.AppRelative(applicationRelativePath);
            this.useDefaultBundleInitializer = useDefaultBundleInitializer;
        }

        protected Bundle()
        {
            // Protected constructor to allow InlineScriptBundle to be created without a path.
        }

        public string Path
        {
            get { return path; }
        }

        public IList<IBundleInitializer> BundleInitializers
        {
            get { return bundleInitializers; }
        }

        public IList<IAsset> Assets
        {
            get { return assets; }
        }

        public string ContentType { get; set; }

        public string Location { get; set; }

        internal byte[] Hash
        {
            get
            {
                if (Assets.Count == 1)
                {
                    return Assets[0].Hash;
                }
                else
                {
                    throw new InvalidOperationException("Bundle Hash is only available when the bundle contains a single asset.");
                }
            }
        }

        internal IEnumerable<string> References
        {
            get { return references; }
        }

        internal virtual void Initialize(ICassetteApplication application)
        {
            if (useDefaultBundleInitializer && bundleInitializers.Count == 0)
            {
                application.GetDefaultBundleInitializer(GetType())
                           .InitializeBundle(this, application);
            }
            else
            {
                foreach (var assetSource in bundleInitializers)
                {
                    assetSource.InitializeBundle(this, application);
                }
            }
        }

        internal virtual void Process(ICassetteApplication application)
        {
        }

        internal abstract IHtmlString Render();

        public void AddAssets(IEnumerable<IAsset> newAssets, bool preSorted)
        {
            foreach (var asset in newAssets)
            {
                assets.Add(asset);
            }
            hasSortedAssets = preSorted;
        }

        internal void AddReferences(IEnumerable<string> referencePathsOrUrls)
        {
            foreach (var reference in referencePathsOrUrls)
            {
                references.Add(ConvertReferenceToAppRelative(reference));
            }
        }

        internal virtual bool ContainsPath(string pathToFind)
        {
            return new BundleContainsPathPredicate().BundleContainsPath(pathToFind, this);
        }

        internal IAsset FindAssetByPath(string pathToFind)
        {
            return Assets.FirstOrDefault(
                a => PathUtilities.PathsEqual(a.SourceFile.FullPath, pathToFind)
            );
        }

        internal void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var asset in assets)
            {
                asset.Accept(visitor);
            }
        }

        internal void SortAssetsByDependency()
        {
            if (hasSortedAssets) return;
            // Graph topological sort, based on references between assets.
            var assetsByFilename = Assets.ToDictionary(
                a => a.SourceFile.FullPath,
                StringComparer.OrdinalIgnoreCase
            );
            var graph = new Graph<IAsset>(
                Assets,
                asset => asset.References
                    .Where(reference => reference.Type == AssetReferenceType.SameBundle)
                    .Select(reference => assetsByFilename[reference.Path])
            );
            var cycles = graph.FindCycles().ToArray();
            if (cycles.Length > 0)
            {
                var details = string.Join(
                    Environment.NewLine,
                    cycles.Select(
                        cycle => "[" + string.Join(", ", cycle.Select(a => a.SourceFile.FullPath)) + "]"
                    )
                );
                throw new InvalidOperationException("Cycles detected in asset references:" + Environment.NewLine + details);
            }
            assets.Clear();
            assets.AddRange(graph.TopologicalSort());
            hasSortedAssets = true;
        }

        internal void ConcatenateAssets()
        {
            var concatenated = new ConcatenatedAsset(assets);
            assets.Clear();
            assets.Add(concatenated);
        }

        string ConvertReferenceToAppRelative(string reference)
        {
            if (reference.IsUrl()) return reference;

            if (reference.StartsWith("~"))
            {
                return PathUtilities.NormalizePath(reference);
            }
            else if (reference.StartsWith("/"))
            {
                return PathUtilities.NormalizePath("~" + reference);
            }
            else
            {
                return PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(
                    Path,
                    reference
                ));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            foreach (var asset in assets.OfType<IDisposable>())
            {
                asset.Dispose();
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
        }
    }
}