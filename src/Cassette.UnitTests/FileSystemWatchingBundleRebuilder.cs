﻿using System;
using System.IO;
using System.Threading;
using Cassette.Configuration;
using Cassette.IO;
using Moq;
using Xunit;

namespace Cassette
{
    public class FileSystemWatchingBundleRebuilder_Tests : IDisposable
    {
        readonly BundleCollection bundles;
        readonly FileSystemWatchingBundleRebuilder rebuilder;
        readonly Mock<IConfiguration<BundleCollection>> bundleConfiguration;
        readonly TempDirectory tempDirectory;

        public FileSystemWatchingBundleRebuilder_Tests()
        {
            tempDirectory = new TempDirectory();
            var settings = new CassetteSettings
            {
                SourceDirectory = new FileSystemDirectory(tempDirectory)
            };
            bundles = new BundleCollection(settings, Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            bundleConfiguration = new Mock<IConfiguration<BundleCollection>>();

            var initializer = new BundleCollectionInitializer(new[] { bundleConfiguration.Object }, new ExternalBundleGenerator(Mock.Of<IBundleFactoryProvider>(), settings));
            rebuilder = new FileSystemWatchingBundleRebuilder(settings, bundles, initializer);
        }

        [Fact]
        public void WhenNewFileCreated_ThenBundleDefinitionIsUsedToRebuildBundleCollection()
        {
            rebuilder.Start();

            File.WriteAllText(Path.Combine(tempDirectory, "test.js"), "");
            Thread.Sleep(200); // Wait for the file system change event to fire.

            bundleConfiguration.Verify(d => d.Configure(bundles), Times.Once());
        }

        [Fact]
        public void WhenFileDeleted_ThenBundleDefinitionIsUsedToRebuildBundleCollection()
        {
            var filename = Path.Combine(tempDirectory, "test.js");
            File.WriteAllText(filename, "");

            rebuilder.Start();

            File.Delete(filename);
            Thread.Sleep(200); // Wait for the file system change event to fire.

            bundleConfiguration.Verify(d => d.Configure(bundles), Times.Once());
        }

        public void Dispose()
        {
            rebuilder.Dispose();
            tempDirectory.Dispose();
        }
    }
}