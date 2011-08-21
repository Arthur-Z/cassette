﻿using System;
using Cassette.Utilities;

namespace Cassette.Web
{
    public class UrlGenerator : IUrlGenerator
    {
        public UrlGenerator(string virtualDirectory, string assetUrlPrefix = "_assets")
        {
            this.virtualDirectory = virtualDirectory.TrimEnd('/');
            this.assetUrlPrefix = assetUrlPrefix;
        }

        readonly string virtualDirectory;
        readonly string assetUrlPrefix;

        public string GetModuleRouteUrl<T>()
        {
            return string.Format(
                "{0}/{1}/{{*path}}",
                assetUrlPrefix,
                ConventionalModulePathName(typeof(T))
            );
        }

        public string GetAssetRouteUrl()
        {
            return assetUrlPrefix + "/get/{*path}";
        }

        public string GetImageRouteUrl()
        {
            return assetUrlPrefix + "/images/{*path}";
        }

        public string CreateModuleUrl(Module module)
        {
            return string.Format("{0}/{1}/{2}/{3}_{4}",
                virtualDirectory,
                assetUrlPrefix,
                ConventionalModulePathName(module.GetType()),
                ConvertToForwardSlashes(module.Path),
                module.Assets[0].Hash.ToHexString()
            );
        }

        public string CreateAssetUrl(Module module, IAsset asset)
        {
            if (module.Path.Length == 0)
            {
                return string.Format(
                    "{0}/{1}?{2}",
                    virtualDirectory,
                    ConvertToForwardSlashes(asset.SourceFilename),
                    asset.Hash.ToHexString()
                );
            }
            else
            {
                return string.Format(
                    "{0}/{1}/{2}?{3}",
                    virtualDirectory,
                    ConvertToForwardSlashes(module.Path),
                    ConvertToForwardSlashes(asset.SourceFilename),
                    asset.Hash.ToHexString()
                );
            }
        }

        public string CreateAssetCompileUrl(Module module, IAsset asset)
        {
            return string.Format(
                "{0}/{1}/get/{2}/{3}?{4}",
                virtualDirectory,
                assetUrlPrefix,
                ConvertToForwardSlashes(module.Path),
                ConvertToForwardSlashes(asset.SourceFilename),
                asset.Hash.ToHexString()
            );
        }

        public string CreateImageUrl(string filename, string hash)
        {
            var dotIndex = filename.LastIndexOf('.');
            var name = filename.Substring(0, dotIndex);
            var extension = filename.Substring(dotIndex + 1);

            return string.Format("{0}/{1}/images/{2}_{3}.{4}",
                virtualDirectory,
                assetUrlPrefix,
                ConvertToForwardSlashes(name),
                hash,
                extension
            );
        }

        string ConventionalModulePathName(Type moduleType)
        {
            var name = moduleType.Name;
            name = name.Substring(0, name.Length - "Module".Length);
            return name.ToLowerInvariant() + "s";
        }

        string ConvertToForwardSlashes(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}