using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette.Configuration
{
    public static class BundleCollectionExtensions
    {
        /// <summary>
        /// Adds a bundle of type <typeparamref name="T"/> using asset files found in the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="bundleCollection">The bundle collection to add to.</param>
        /// <param name="applicationRelativePath">The application relative path to the bundle's asset files.</param>
        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath)
            where T : Bundle
        {
            Add<T>(bundleCollection, applicationRelativePath, null, null);
        }

        /// <summary>
        /// Adds a bundle of type <typeparamref name="T"/> using asset files found in the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="bundleCollection">The bundle collection to add to.</param>
        /// <param name="applicationRelativePath">The application relative path to the bundle's asset files.</param>
        /// <param name="fileSearch">The file search used to find asset files to include in the bundle.</param>
        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSearch fileSearch)
            where T : Bundle
        {
            Add<T>(bundleCollection, applicationRelativePath, fileSearch, null);
        }

        /// <summary>
        /// Adds a bundle of type <typeparamref name="T"/> using asset files found in the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="bundleCollection">The bundle collection to add to.</param>
        /// <param name="applicationRelativePath">The application relative path to the bundle's asset files.</param>
        /// <param name="customizeBundle">The delegate used to customize the created bundle before adding it to the collection.</param>
        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, Action<T> customizeBundle)
            where T : Bundle
        {
            Add(bundleCollection, applicationRelativePath, null, customizeBundle);
        }

        /// <summary>
        /// Adds a bundle of type <typeparamref name="T"/> using asset files found in the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="bundleCollection">The bundle collection to add to.</param>
        /// <param name="applicationRelativePath">The application relative path to the bundle's asset files.</param>
        /// <param name="fileSearch">The file search used to find asset files to include in the bundle.</param>
        /// <param name="customizeBundle">The delegate used to customize the created bundle before adding it to the collection.</param>
        public static void Add<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSearch fileSearch, Action<T> customizeBundle)
            where T : Bundle
        {
            Trace.Source.TraceInformation(string.Format("Creating {0} for {1}", typeof(T).Name, applicationRelativePath));

            T bundle;
            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            
            var source = bundleCollection.Settings.SourceDirectory;
            if (source.DirectoryExists(applicationRelativePath))
            {
                fileSearch = fileSearch ?? bundleCollection.Settings.DefaultFileSearches[typeof(T)];
                var directory = source.GetDirectory(applicationRelativePath);
                var allFiles = fileSearch.FindFiles(directory);
                bundle = CreateDirectoryBundle(applicationRelativePath, bundleFactory, allFiles, directory);
            }
            else
            {
                var file = source.GetFile(applicationRelativePath);
                if (file.Exists)
                {
                    bundle = CreateSingleFileBundle(applicationRelativePath, file, bundleFactory);
                }
                else
                {
                    throw new DirectoryNotFoundException(string.Format("Bundle path not found: {0}", applicationRelativePath));
                }
            }

            if (customizeBundle != null)
            {
                customizeBundle(bundle);
            }

            TraceAssetFilePaths(bundle);

            bundleCollection.Add(bundle);
        }

        internal static T CreateSingleFileBundle<T>(
            string applicationRelativePath,
            IFile file,
            IBundleFactory<T> bundleFactory,
            BundleDescriptor descriptor = null) where T : Bundle
        {
            descriptor = descriptor ?? new BundleDescriptor
            {
                AssetFilenames = { applicationRelativePath }
            };
            return bundleFactory.CreateBundle(applicationRelativePath, new[] { file }, descriptor);
        }

        internal static T CreateDirectoryBundle<T>(
            string applicationRelativePath,
            IBundleFactory<T> bundleFactory,
            IEnumerable<IFile> allFiles,
            IDirectory directory,
            BundleDescriptor descriptor = null) where T : Bundle
        {
            var descriptorFile = TryGetDescriptorFile(directory);
            if (descriptor == null)
            {
                descriptor = descriptorFile.Exists
                    ? new BundleDescriptorReader(descriptorFile).Read()
                    : new BundleDescriptor { AssetFilenames = { "*" } };
            }
            return bundleFactory.CreateBundle(applicationRelativePath, allFiles, descriptor);
        }

        static IFile TryGetDescriptorFile(IDirectory directory)
        {
            var descriptorFile = directory.GetFile("bundle.txt");

            // TODO: Remove this legacy support for module.txt
            if (!descriptorFile.Exists) descriptorFile = directory.GetFile("module.txt");

            return descriptorFile;
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory<T>(bundleCollection, applicationRelativePath, null, null, excludeTopLevel);
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="fileSearch">A file source that gets the files to include from a directory.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the directory, if any.</param>
        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSearch fileSearch, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory<T>(bundleCollection, applicationRelativePath, fileSearch, null, excludeTopLevel);            
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, Action<T> customizeBundle, bool excludeTopLevel = false)
            where T : Bundle
        {
            AddPerSubDirectory(bundleCollection, applicationRelativePath, null, customizeBundle, excludeTopLevel);
        }

        /// <summary>
        /// Adds a bundle for each sub-directory of the given path.
        /// </summary>
        /// <typeparam name="T">The type of bundles to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="applicationRelativePath">The path to the directory containing sub-directories.</param>
        /// <param name="fileSearch">A file source that gets the files to include from a directory.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <param name="excludeTopLevel">Prevents the creation of an extra bundle from the top-level files of the path, if any.</param>
        public static void AddPerSubDirectory<T>(this BundleCollection bundleCollection, string applicationRelativePath, IFileSearch fileSearch, Action<T> customizeBundle, bool excludeTopLevel = false)
            where T : Bundle
        {
            Trace.Source.TraceInformation(string.Format("Creating {0} for each subdirectory of {1}", typeof(T).Name, applicationRelativePath));

            fileSearch = fileSearch ?? bundleCollection.Settings.DefaultFileSearches[typeof(T)];

            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            var parentDirectory = bundleCollection.Settings.SourceDirectory.GetDirectory(applicationRelativePath);

            if (!excludeTopLevel)
            {
                var topLevelFiles = fileSearch.FindFiles(parentDirectory)
                                              .Where(f => f.Directory == parentDirectory)
                                              .ToArray();
                if (topLevelFiles.Any())
                {
                    var directoryBundle = CreateDirectoryBundle(applicationRelativePath, bundleFactory, topLevelFiles, parentDirectory);
                    bundleCollection.Add(directoryBundle);
                }
            }

            var directories = parentDirectory.GetDirectories().Where(IsNotHidden);
            foreach (var directory in directories)
            {
                Trace.Source.TraceInformation(string.Format("Creating {0} for {1}", typeof(T).Name, applicationRelativePath));
                var descriptorFile = TryGetDescriptorFile(directory);
                var descriptor = descriptorFile.Exists
                    ? new BundleDescriptorReader(descriptorFile).Read()
                    : new BundleDescriptor { AssetFilenames = { "*" } };
                var allFiles = fileSearch.FindFiles(directory);
                var bundle = bundleFactory.CreateBundle(directory.FullPath, allFiles, descriptor);
                if (customizeBundle != null) customizeBundle(bundle);
                TraceAssetFilePaths(bundle);
                bundleCollection.Add(bundle);
            }
        }

        /// <summary>
        /// Adds a bundle that references a URL instead of local asset files.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="url">The URL to reference.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <returns>A object used to further configure the bundle.</returns>
        public static IAddUrlConfiguration AddUrl<T>(this BundleCollection bundleCollection, string url, Action<T> customizeBundle = null)
            where T : Bundle
        {
            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            var bundle = bundleFactory.CreateExternalBundle(url);
            if (customizeBundle != null) customizeBundle(bundle);
            bundleCollection.Add(bundle);
            return new AddUrlConfiguration<T>(bundleCollection, bundle, url, customizeBundle);
        }

        /// <summary>
        /// Adds a bundle that references a URL instead of local asset files. The type of bundle created is determined by the URL's file extension.
        /// </summary>
        /// <param name="bundleCollection">The collection to add to.</param>
        /// <param name="url">The URL to reference.</param>
        /// <param name="customizeBundle">A delegate that is called for each created bundle to allow customization.</param>
        /// <returns>A object used to further configure the bundle.</returns>
        public static IAddUrlConfiguration AddUrl(this BundleCollection bundleCollection, string url, Action<Bundle> customizeBundle = null)
        {
            if (url.EndsWith(".js"))
            {
                return AddUrl<Scripts.ScriptBundle>(bundleCollection, url, customizeBundle);
            }
            else if (url.EndsWith(".css"))
            {
                return AddUrl<Stylesheets.StylesheetBundle>(bundleCollection, url, customizeBundle);
            }
            else
            {
                throw new ArgumentException("Cannot determine the type of bundle to add. Specify the type using the generic overload of this method.");
            }
        }

        /// <summary>
        /// Adds a bundle for each individual file found using the file search. If no file search is provided the application
        /// default file search for the bundle type is used.
        /// </summary>
        /// <typeparam name="T">The type of bundle to create.</typeparam>
        /// <param name="bundleCollection">The bundle collection to add to.</param>
        /// <param name="directoryPath">The path to the directory to search. If null or empty the application source directory is used.</param>
        /// <param name="fileSearch">The <see cref="IFileSearch"/> used to find files. If null the application default file search for the bundle type is used.</param>
        /// <param name="customizeBundle">An optional action delegate called for each bundle.</param>
        public static void AddPerIndividualFile<T>(this BundleCollection bundleCollection, string directoryPath = null, IFileSearch fileSearch = null, Action<T> customizeBundle = null)
            where T : Bundle
        {
            var directory = string.IsNullOrEmpty(directoryPath)
                ? bundleCollection.Settings.SourceDirectory
                : bundleCollection.Settings.SourceDirectory.GetDirectory(directoryPath);

            fileSearch = fileSearch ?? bundleCollection.Settings.DefaultFileSearches[typeof(T)];
            var files = fileSearch.FindFiles(directory);
            var bundleFactory = (IBundleFactory<T>)bundleCollection.Settings.BundleFactories[typeof(T)];
            foreach (var file in files)
            {
                var bundle = bundleFactory.CreateBundle(
                    file.FullPath,
                    new[] { file },
                    new BundleDescriptor { AssetFilenames = { "*" } }
                );
                if (customizeBundle != null) customizeBundle(bundle);
                bundleCollection.Add(bundle);
            }
        }

        static void TraceAssetFilePaths<T>(T bundle) where T : Bundle
        {
            foreach (var asset in bundle.Assets)
            {
                Trace.Source.TraceInformation(string.Format("Added asset {0}", asset.SourceFile.FullPath));
            }
        }

        static bool IsNotHidden(IDirectory directory)
        {
            return !directory.Attributes.HasFlag(FileAttributes.Hidden);
        }
    }
}