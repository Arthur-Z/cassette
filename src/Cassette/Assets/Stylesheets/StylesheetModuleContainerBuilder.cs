﻿using System.IO.IsolatedStorage;
using System.Linq;

namespace Cassette.Assets.Stylesheets
{
    public class StylesheetModuleContainerBuilder : ModuleContainerBuilder
    {
        readonly string applicationRoot;

        public StylesheetModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory, string applicationRoot)
            : base(storage, rootDirectory)
        {
            this.applicationRoot = EnsureDirectoryEndsWithSlash(applicationRoot);
        }

        public override ModuleContainer Build()
        {
            var moduleBuilder = new UnresolvedStylesheetModuleBuilder(rootDirectory, applicationRoot);
            var unresolvedModules = relativeModuleDirectories.Select(x => moduleBuilder.Build(x.Item1, x.Item2));
            var modules = UnresolvedModule.ResolveAll(unresolvedModules);
            return new ModuleContainer(
                modules, 
                storage, 
                textWriter => new StylesheetModuleWriter(textWriter, rootDirectory, applicationRoot, LoadFile)
            );
        }
    }
}
