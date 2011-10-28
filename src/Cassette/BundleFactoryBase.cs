using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette
{
    abstract class BundleFactoryBase<T> : IBundleFactory<T> 
        where T : Bundle
    {
        public virtual T CreateBundle(string path, IEnumerable<IFile> allFiles, BundleDescriptor bundleDescriptor)
        {
            var bundle = CreateBundleCore(path, bundleDescriptor);
            var filesArray = allFiles.ToArray();
            AddAssets(bundle, filesArray, bundleDescriptor.AssetFilenames);
            AddReferences(bundle, bundleDescriptor.References);
            SetIsSortedIfExplicitFilenames(bundle, bundleDescriptor.AssetFilenames);
            return bundle;
        }

        protected abstract T CreateBundleCore(string path, BundleDescriptor bundleDescriptor);

        void AddAssets(Bundle bundle, IFile[] allFiles, IEnumerable<string> filenames)
        {
            var remainingFiles = new HashSet<IFile>(allFiles);
            var filesByPath = allFiles.ToDictionary(f => f.FullPath);

            foreach (var filename in filenames)
            {
                if (filename == "*")
                {
                    AddAllAssetsToBundle(bundle, remainingFiles);
                    break;
                }
                else
                {
                    var file = FindFileOrThrow(bundle, filename, filesByPath);

                    bundle.Assets.Add(new Asset(file, bundle));
                    remainingFiles.Remove(file);
                }
            }
        }

        IFile FindFileOrThrow(Bundle bundle, string filename, Dictionary<string, IFile> filesByPath)
        {
            IFile file;
            if (filesByPath.TryGetValue(filename, out file))
            {
                return file;
            }

            throw new FileNotFoundException(
                string.Format(
                    "The asset file \"{0}\" was not found for bundle \"{1}\".",
                    filename,
                    bundle.Path
                )
            );
        }

        void AddAllAssetsToBundle(Bundle bundle, IEnumerable<IFile> remainingFiles)
        {
            foreach (var file in remainingFiles)
            {
                bundle.Assets.Add(new Asset(file, bundle));
            }
        }

        void AddReferences(Bundle bundle, IEnumerable<string> references)
        {
            foreach (var reference in references)
            {
                bundle.AddReference(reference);
            }
        }

        void SetIsSortedIfExplicitFilenames(Bundle bundle, IList<string> filenames)
        {
            if (filenames.Count == 0 || filenames[0] != "*")
            {
                bundle.IsSorted = true;
            }
        }
    }
}