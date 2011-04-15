﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Knapsack
{
    public class ModuleContainerBuilder
    {
        public ModuleContainerBuilder(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        readonly string rootDirectory;
        readonly List<string> relativeModuleDirectories = new List<string>();

        public void AddModule(string relativeDirectory)
        {
            relativeModuleDirectories.Add(relativeDirectory);
        }

        public void AddModuleForEachSubdirectoryOf(string directory)
        {
            directory = directory.ToLower();
            foreach (var path in Directory.GetDirectories(Path.Combine(rootDirectory, directory)))
            {
                AddModule(path.Substring(rootDirectory.Length));
            }
        }

        public ModuleContainer Build()
        {
            var modules = relativeModuleDirectories.Select(LoadUnresolvedModule);
            return new ModuleContainer(UnresolvedModule.ResolveAll(modules));
        }

        UnresolvedModule LoadUnresolvedModule(string relativeDirectory)
        {
            var path = Path.Combine(rootDirectory, relativeDirectory);
            var filenames = Directory.GetFiles(path, "*.js").Where(f => !f.EndsWith("-vsdoc.js"));
            return new UnresolvedModule(relativeDirectory, filenames.Select(LoadScript).ToArray());
        }

        UnresolvedScript LoadScript(string filename)
        {
            var parser = new ScriptParser();
            using (var fileStream = File.OpenRead(filename))
            {
                return parser.Parse(fileStream, filename.Substring(rootDirectory.Length));
            }
        }
    }
}
