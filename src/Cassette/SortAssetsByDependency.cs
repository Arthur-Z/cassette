﻿using System;
using System.IO;
using System.Linq;
using Cassette.Utilities;

namespace Cassette
{
    public class SortAssetsByDependency<T> : IModuleProcessor<T>
        where T : Module
    {
        public void Process(T module)
        {
            // In the absence of dependencies, sort by the filename to ensure consistent output.
            var sortedByFilename = module.Assets.OrderBy(
                a => Path.Combine(module.Directory, a.SourceFilename)
            );

            // Graph topological sort, based on references between assets.
            var assetsByFilename = module.Assets.ToDictionary(
                a => Path.Combine(module.Directory, a.SourceFilename),
                StringComparer.OrdinalIgnoreCase
            );
            var graph = new Graph<IAsset>(
                sortedByFilename,
                asset => asset.References
                    .Where(reference => reference.Type == AssetReferenceType.SameModule)
                    .Select(reference => assetsByFilename[reference.ReferencedFilename])
            );
            module.Assets = graph.TopologicalSort().ToList();
        }

    }
}
