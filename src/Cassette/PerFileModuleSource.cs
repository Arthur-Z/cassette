﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette
{
    public class PerFileModuleSource<T> : IModuleSource<T>
        where T : Module
    {
        readonly string basePath;

        public PerFileModuleSource(string basePath)
        {
            if (basePath.StartsWith("~"))
            {
                this.basePath = basePath.TrimEnd('/', '\\');
            }
            else if (basePath.StartsWith("/"))
            {
                this.basePath = "~";
            }
            else
            {
                this.basePath = "~/" + basePath.TrimEnd('/', '\\');
            }
        }

        public string FilePattern { get; set; }

        public Regex Exclude { get; set; }

        public IEnumerable<T> GetModules(IModuleFactory<T> moduleFactory, ICassetteApplication application)
        {
            var directory = GetDirectoryForBasePath(application);
            var filenames = GetFilenames(directory);

            return from filename in filenames
                   select CreateModule(filename, moduleFactory, directory);
        }

        IFileSystem GetDirectoryForBasePath(ICassetteApplication application)
        {
            return basePath.Length == 1
                       ? application.RootDirectory
                       : application.RootDirectory.NavigateTo(basePath.Substring(2), false);
        }

        IEnumerable<string> GetFilenames(IFileSystem directory)
        {
            IEnumerable<string> filenames;
            if (string.IsNullOrEmpty(FilePattern))
            {
                filenames = directory.GetFiles("");
            }
            else
            {
                var patterns = FilePattern.Split(';', ',');
                filenames = patterns.SelectMany(
                    pattern => directory.GetFiles("", pattern)
                );
            }

            if (Exclude != null)
            {
                filenames = filenames.Where(f => Exclude.IsMatch(f) == false);
            }
            return filenames;
        }

        T CreateModule(string filename, IModuleFactory<T> moduleFactory, IFileSystem directory)
        {
            var name = RemoveFileExtension(filename);
            var module = moduleFactory.CreateModule(PathUtilities.CombineWithForwardSlashes(basePath, name));
            var asset = new Asset(
                PathUtilities.CombineWithForwardSlashes(basePath, filename),
                module,
                directory.GetFile(filename)
            );
            module.AddAssets(new[] { asset }, true);
            return module;
        }

        string RemoveFileExtension(string filename)
        {
            var index = filename.LastIndexOf('.');
            return (index >= 0) ? filename.Substring(0, index) : filename;
        }
    }
}
