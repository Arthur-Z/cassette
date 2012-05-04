﻿using System;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Caching
{
    public class BundleCollectionCache_Write_Tests : IDisposable
    {
        readonly TempDirectory path;
        readonly FileSystemDirectory directory;
        readonly Mock<ScriptBundle> scriptBundle;
        readonly Mock<StylesheetBundle> stylesheetBundle;

        public BundleCollectionCache_Write_Tests()
        {
            path = new TempDirectory();
            directory = new FileSystemDirectory(path);

            var bundles = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            scriptBundle = new Mock<ScriptBundle>("~/test1");
            scriptBundle.Object.Hash = new byte[] { 1, 2, 3 };
            scriptBundle.Object.Assets.Add(new StubAsset("~/test/asset.js", "script-bundle-content"));
            bundles.Add(scriptBundle.Object);
            
            stylesheetBundle = new Mock<StylesheetBundle>("~/test2");
            stylesheetBundle.Object.Hash = new byte[] { 4, 5, 6 };
            stylesheetBundle.Object.Assets.Add(new StubAsset("~/test2/asset.css", "stylesheet-bundle-content"));
            bundles.Add(stylesheetBundle.Object);

            var cache = new BundleCollectionCache(directory, b => null);
            cache.Write(bundles, "VERSION");
        }

        [Fact]
        public void ItCreatesManifestXmlFile()
        {
            directory.GetFile("manifest.xml").Exists.ShouldBeTrue();
        }

        [Fact]
        public void CreatedManifestXmlHasVersionAttribute()
        {
            var xml = directory.GetFile("manifest.xml").OpenRead().ReadToEnd();
            xml.ShouldContain("Version=\"VERSION\"");
        }

        [Fact]
        public void ItSerializesBundlesIntoManifest()
        {
            scriptBundle.Verify(b => b.SerializeInto(It.IsAny<XContainer>()));
            stylesheetBundle.Verify(b => b.SerializeInto(It.IsAny<XContainer>()));
        }

        [Fact]
        public void ItCreatesScriptBundleContentFile()
        {
            var file = directory.GetFile("~/010203.js");
            var content = file.OpenRead().ReadToEnd();
            content.ShouldEqual("script-bundle-content");
        }

        [Fact]
        public void ItCreatesStylesheetBundleContentFile()
        {
            var file = directory.GetFile("~/040506.css");
            var content = file.OpenRead().ReadToEnd();
            content.ShouldEqual("stylesheet-bundle-content");
        }

        public void Dispose()
        {
            path.Dispose();
        }
    }
}