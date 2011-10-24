﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class BundleCollectionExtensions_Tests
    {
        [Fact]
        public void GivenTwoBundleDirectories_WhenAddForEachSubDirectory_ThenTwoBundlesAreAddedToTheCollection()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle-a"));
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle-b"));

                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory)
                };
            
                var factory = new Mock<IBundleFactory<Bundle>>();
                var bundles = new Queue<Bundle>(new[] { new TestableBundle("~/test/bundle-a"), new TestableBundle("~/test/bundle-b") });
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(bundles.Dequeue);
                settings.BundleFactories[typeof(Bundle)] = factory.Object;

                var collection = new BundleCollection(settings);
                collection.AddForEachSubDirectory<Bundle>("~/test");

                var result = collection.ToArray();
                result[0].Path.ShouldEqual("~/test/bundle-a");
                result[1].Path.ShouldEqual("~/test/bundle-b");
            }
        }

        [Fact]
        public void GivenBundleDirectoryWithDescriptorFile_WhenAddForEachSubDirectory_ThenDescriptorPassedToBundleFactory()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle"));
                File.WriteAllText(Path.Combine(tempDirectory, "test", "bundle", "bundle.txt"), "");
                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory)
                };
                var factory = new Mock<IBundleFactory<Bundle>>();
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(new TestableBundle("~/test/bundle"));
                settings.BundleFactories[typeof(Bundle)] = factory.Object;

                var collection = new BundleCollection(settings);
                collection.AddForEachSubDirectory<Bundle>("~/test");
                
                factory.Verify(f => f.CreateBundle("~/test/bundle", It.Is<BundleDescriptor>(b => b != null)));
            }
        }

        [Fact]
        public void WhenAddForEachSubDirectoryWithInitializer_ThenBundleInitializerIsAssignedForCreatedBundle()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test"));

                var factory = new Mock<IBundleFactory<Bundle>>();
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(new TestableBundle("~/test"));
                
                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory),
                    BundleFactories = {{typeof(Bundle), factory.Object}}
                };
                var bundles = new BundleCollection(settings);
                var initializer = Mock.Of<IBundleInitializer>();

                bundles.AddForEachSubDirectory<Bundle>("~/", initializer);

                bundles["~/test"].BundleInitializers.ShouldContain(initializer);
            }
        }

        [Fact]
        public void WhenAddForeachSubDirectoryWithBundleCustomization_ThenBundleIsCustomized()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test"));

                var factory = new Mock<IBundleFactory<Bundle>>();
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(new TestableBundle("~/test"));

                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory),
                    BundleFactories = { { typeof(Bundle), factory.Object } }
                };
                var bundles = new BundleCollection(settings);
                
                bundles.AddForEachSubDirectory<Bundle>("~/", bundle => bundle.ContentType = "TEST");

                bundles["~/test"].ContentType.ShouldEqual("TEST");
            }
        }

        [Fact]
        public void WhenAddForeachSubDirectoryWithBundleInitializerAndCustomization_ThenBundleHasInitializerAndIsCustomized()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test"));

                var factory = new Mock<IBundleFactory<Bundle>>();
                factory.Setup(f => f.CreateBundle(It.IsAny<string>(), It.IsAny<BundleDescriptor>()))
                       .Returns(new TestableBundle("~/test"));

                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory),
                    BundleFactories = { { typeof(Bundle), factory.Object } }
                };
                var bundles = new BundleCollection(settings);
                var initializer = Mock.Of<IBundleInitializer>();

                bundles.AddForEachSubDirectory<Bundle>("~/", initializer, bundle => bundle.ContentType = "TEST");

                bundles["~/test"].BundleInitializers.ShouldContain(initializer);
                bundles["~/test"].ContentType.ShouldEqual("TEST");
            }
        }

        [Fact]
        public void GivenHiddenDirectory_WhenAddForEachSubDirectory_ThenDirectoryIsIgnored()
        {
            using (var tempDirectory = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(tempDirectory, "test", "bundle"));
                var attributes = File.GetAttributes(Path.Combine(tempDirectory, "test", "bundle"));
                File.SetAttributes(Path.Combine(tempDirectory, "test", "bundle"), attributes | FileAttributes.Hidden);
                
                var settings = new CassetteSettings
                {
                    SourceDirectory = new FileSystemDirectory(tempDirectory)
                };
                settings.BundleFactories[typeof(Bundle)] = Mock.Of<IBundleFactory<Bundle>>();

                var collection = new BundleCollection(settings);
                collection.AddForEachSubDirectory<Bundle>("~/test");

                collection.ShouldBeEmpty();
            }
        }
    }
}