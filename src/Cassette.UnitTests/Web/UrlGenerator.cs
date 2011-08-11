﻿using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class UrlGenerator_CreateModuleUrl_Tests
    {
        ScriptModule StubScriptModule(string path)
        {
            var module = new ScriptModule(path, Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            module.Assets.Add(asset.Object);
            return module;
        }

        StylesheetModule StubStylesheetModule(string path)
        {
            var module = new StylesheetModule(path, Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            module.Assets.Add(asset.Object);
            return module;
        }

        [Fact]
        public void UrlStartsWithApplicationVirtualDirectory()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/");
        }

        [Fact]
        public void AppendsSlashToVirtualDirectoryWhenMissingFromEnd()
        {
            var app = new UrlGenerator("/myapp");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/myapp/");
        }

        [Fact]
        public void Inserts_assetsPrefix()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/_assets/");
        }

        [Fact]
        public void InsertsLowercasedPluralisedScriptModuleTypeName()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/_assets/scripts/");
        }

        [Fact]
        public void InsertsLowercasedPluralisedStylesheetModuleTypeName()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubStylesheetModule("test"));
            url.ShouldStartWith("/_assets/stylesheets/");
        }

        [Fact]
        public void InsertsModuleDirectory()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/_assets/scripts/test");
        }

        [Fact]
        public void InsertsModuleDirectoryWithBackSlashesConvertedToForwardSlashes()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test\\foo\\bar"));
            url.ShouldStartWith("/_assets/scripts/test/foo/bar");
        }

        [Fact]
        public void AppendsModuleHashHexString()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test\\foo\\bar"));
            url.ShouldEqual("/_assets/scripts/test/foo/bar_010203");
        }
    }

    public class UrlGenerator_CreateAssetUrl_Tests
    {
        [Fact]
        public void StartsWithApplicationVirtualDirectory()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/");
        }

        [Fact]
        public void StartsWithApplicationVirtualDirectoryEndingInSlash()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/");
        }

        [Fact]
        public void InsertsModuleDirectory()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/test/");
        }

        [Fact]
        public void InsertsModuleDirectoryWithBackSlashesConvertedToForwardSlashes()
        {
            var module = new Module("test\\foo\\bar", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/test/foo/bar");
        }

        [Fact]
        public void InsertsAssetSourceFilename()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/test/asset.js");
        }

        [Fact]
        public void InsertsAssetSourceFilenameWithBackSlashesConvertedToForwardSlashes()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("sub\\asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/test/sub/asset.js");
        }

        [Fact]
        public void AppendsHashHexString()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("sub\\asset.js");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldEqual("/test/sub/asset.js?01020f10");
        }
    }

    public class UrlGenerator_ModuleUrlPattern_Tests
    {
        [Fact]
        public void InsertsConventionalScriptModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.ModuleUrlPattern<ScriptModule>();
            url.ShouldEqual("_assets/scripts/{*path}");
        }

        [Fact]
        public void InsertsConventionalStylesheetModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.ModuleUrlPattern<StylesheetModule>();
            url.ShouldEqual("_assets/stylesheets/{*path}");
        }

        [Fact]
        public void InsertsConventionalHtmlTemplateModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.ModuleUrlPattern<HtmlTemplateModule>();
            url.ShouldEqual("_assets/htmltemplates/{*path}");
        }
    }
}
