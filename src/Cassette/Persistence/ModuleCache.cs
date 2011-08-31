﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Cassette.Persistence
{
    public class ModuleCache<T> : IModuleCache<T>
        where T : Module
    {
        public ModuleCache(IFileSystem cacheDirectory, IModuleFactory<T> moduleFactory)
        {
            this.cacheDirectory = cacheDirectory;
            this.moduleFactory = moduleFactory;
        }

        readonly IFileSystem cacheDirectory;
        readonly IModuleFactory<T> moduleFactory;
        const string ContainerFilename = "container.xml";
        const string VersionFilename = "version";

        public bool LoadContainerIfUpToDate(IEnumerable<T> externalModules, int expectedAssetCount, string version, IFileSystem sourceFileSystem, out IModuleContainer<T> container)
        {
            container = null;
            if (!cacheDirectory.FileExists(ContainerFilename)) return false;
            if (!cacheDirectory.FileExists(VersionFilename)) return false;
            if (!IsSameVersion(version)) return false;

            var lastWriteTime = cacheDirectory.GetLastWriteTimeUtc(ContainerFilename);
            
            var modules = LoadModules().Concat(externalModules);
            var cachedContainer = new CachedModuleContainer<T>(modules);
            if (cachedContainer.IsUpToDate(expectedAssetCount, lastWriteTime, sourceFileSystem))
            {
                container = cachedContainer;
                return true;
            }
            
            return false;
        }

        bool IsSameVersion(string version)
        {
            using (var reader = new StreamReader(cacheDirectory.OpenFile(VersionFilename, FileMode.Open, FileAccess.Read)))
            {
                return reader.ReadLine() == version;
            }
        }

        IEnumerable<T> LoadModules()
        {
            var containerElement = LoadContainerElement(cacheDirectory);
            var reader = new ModuleManifestReader<T>(cacheDirectory, moduleFactory);
            return reader.CreateModules(containerElement);
        }

        public IModuleContainer<T> SaveModuleContainer(IEnumerable<T> modules, string version)
        {
            cacheDirectory.DeleteAll();
            SaveContainerXml(modules);
            SaveVersion(version);
            foreach (var module in modules)
            {
                SaveModule(module);
            }
            return new CachedModuleContainer<T>(modules);
        }

        XElement LoadContainerElement(IFileSystem fileSystem)
        {
            using (var containerFile = fileSystem.OpenFile(ContainerFilename, FileMode.Open, FileAccess.Read))
            {
                return XDocument.Load(containerFile).Root;
            }
        }

        void SaveContainerXml(IEnumerable<T> modules)
        {
            var xml = new XDocument(
                new XElement("container",
                    modules.SelectMany(module => module.CreateCacheManifest())
                )
            );
            using (var fileStream = cacheDirectory.OpenFile(ContainerFilename, FileMode.Create, FileAccess.Write))
            {
                xml.Save(fileStream);
            }
        }

        void SaveVersion(string version)
        {
            using (var writer = new StreamWriter(cacheDirectory.OpenFile(VersionFilename, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(version);
            }
        }

        void SaveModule(T module)
        {
            if (module.Assets.Count > 1)
            {
                throw new InvalidOperationException("Cannot cache a module when assets have not been concatenated into a single asset.");
            }
            if (module.Assets.Count == 0)
            {
                return; // Skip external URL modules.
            }

            var filename = ModuleAssetCacheFilename(module);
            using (var fileStream = cacheDirectory.OpenFile(filename, FileMode.Create, FileAccess.Write))
            {
                using (var dataStream = module.Assets[0].OpenStream())
                {
                    dataStream.CopyTo(fileStream);
                }
                fileStream.Flush();
            }
        }

        internal static string ModuleAssetCacheFilename(Module module)
        {
            return module.Path.Replace(Path.DirectorySeparatorChar, '`')
                       .Replace(Path.AltDirectorySeparatorChar, '`') 
                 + ".module";
        }
    }
}
