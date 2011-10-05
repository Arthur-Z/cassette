﻿#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;
using System;

namespace Cassette.Web
{
    public class UrlGenerator_CreateModuleUrl_Tests
    {
        [Fact]
        public void UrlStartsWithApplicationVirtualDirectory()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("~/test"));
            url.ShouldStartWith("/");
        }

        [Fact]
        public void AppendsSlashToVirtualDirectoryWhenMissingFromEnd()
        {
            var app = new UrlGenerator("/myapp");
            var url = app.CreateModuleUrl(StubScriptModule("~/test"));
            url.ShouldStartWith("/myapp/");
        }

        [Fact]
        public void Inserts_assetsPrefix()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("~/test"));
            url.ShouldStartWith("/_assets/");
        }

        [Fact]
        public void InsertsLowercasedPluralisedScriptModuleTypeName()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("~/test"));
            url.ShouldStartWith("/_assets/scripts/");
        }

        [Fact]
        public void InsertsLowercasedPluralisedStylesheetModuleTypeName()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubStylesheetModule("~/test"));
            url.ShouldStartWith("/_assets/stylesheets/");
        }

        [Fact]
        public void InsertsModuleDirectory()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("~/test"));
            url.ShouldStartWith("/_assets/scripts/test");
        }

        [Fact]
        public void InsertsModuleDirectoryWithBackSlashesConvertedToForwardSlashes()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("~\\test\\foo\\bar"));
            url.ShouldStartWith("/_assets/scripts/test/foo/bar");
        }

        [Fact]
        public void AppendsModuleHashHexString()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("~\\test\\foo\\bar"));
            url.ShouldEqual("/_assets/scripts/test/foo/bar_010203");
        }

        ScriptModule StubScriptModule(string path)
        {
            var module = new ScriptModule(path);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            module.Assets.Add(asset.Object);
            return module;
        }

        StylesheetModule StubStylesheetModule(string path)
        {
            var module = new StylesheetModule(path);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            module.Assets.Add(asset.Object);
            return module;
        }

    }

    public class UrlGenerator_CreateAssetUrl_Tests
    {
        [Fact]
        public void StartsWithApplicationVirtualDirectory()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(asset.Object);

            url.ShouldStartWith("/");
        }

        [Fact]
        public void StartsWithApplicationVirtualDirectoryEndingInSlash()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(asset.Object);

            url.ShouldStartWith("/myapp/");
        }

        [Fact]
        public void InsertsModuleDirectory()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(asset.Object);

            url.ShouldStartWith("/myapp/test/");
        }

        [Fact]
        public void InsertsModuleDirectoryWithBackSlashesConvertedToForwardSlashes()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/foo/bar/asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(asset.Object);

            url.ShouldStartWith("/myapp/test/foo/bar");
        }

        [Fact]
        public void InsertsAssetSourceFilename()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(asset.Object);

            url.ShouldStartWith("/test/asset.js");
        }

        [Fact]
        public void InsertsAssetSourceFilenameWithBackSlashesConvertedToForwardSlashes()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/sub/asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(asset.Object);

            url.ShouldStartWith("/test/sub/asset.js");
        }

        [Fact]
        public void AppendsHashHexString()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/sub/asset.js");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(asset.Object);

            url.ShouldEqual("/test/sub/asset.js?01020f10");
        }

        [Fact]
        public void CreateAssetCompileUrlReturnsCompileUrl()
        {
            var module = new Module("~/test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("~/test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
            var app = new UrlGenerator("/");

            var url = app.CreateAssetCompileUrl(module, asset.Object);

            url.ShouldEqual("/_assets/get/test/asset.coffee?01020f10");
        }
    }

    public class UrlGenerator_CreateImageUrl_Tests
    {
        [Fact]
        public void CreateImageUrlPrependsHandlerRoute()
        {
            var generator = new UrlGenerator("/");
            var url = generator.CreateRawFileUrl("~/test.png", "hash");
            url.ShouldStartWith("/_assets/images/");
        }

        [Fact]
        public void CreateImageUrlConvertsFileExtensinDotToUnderscore()
        {
            var generator = new UrlGenerator("/");
            var url = generator.CreateRawFileUrl("~/test.png", "hash");
            url.ShouldStartWith("/_assets/images/test_hash_png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            var generator = new UrlGenerator("/");
            var url = generator.CreateRawFileUrl("~\\test\\foo.png", "hash");
            url.ShouldEqual("/_assets/images/test/foo_hash_png");
        }

        [Fact]
        public void ArgumentExceptionThrownWhenFilenameDoesNotStartWithTilde()
        {
            var generator = new UrlGenerator("/");
            Assert.Throws<ArgumentException>(delegate
            {
                generator.CreateRawFileUrl("fail.png", "hash");
            });
        }
    }

    public class UrlGenerator_RouteUrl_Tests
    {
        [Fact]
        public void GetModuleRouteUrl_InsertsConventionalScriptModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.GetModuleRouteUrl<ScriptModule>();
            url.ShouldEqual("_assets/scripts/{*path}");
        }

        [Fact]
        public void GetModuleRouteUrl_InsertsConventionalExternalScriptModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.GetModuleRouteUrl<ExternalScriptModule>();
            url.ShouldEqual("_assets/scripts/{*path}");
        }

        [Fact]
        public void GetModuleRouteUrl_InsertsConventionalStylesheetModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.GetModuleRouteUrl<StylesheetModule>();
            url.ShouldEqual("_assets/stylesheets/{*path}");
        }

        [Fact]
        public void GetModuleRouteUrl_InsertsConventionalHtmlTemplateModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.GetModuleRouteUrl<HtmlTemplateModule>();
            url.ShouldEqual("_assets/htmltemplates/{*path}");
        }

        [Fact]
        public void GetAssetRouteUrl_ReturnsRouteUrlWithPathParameter()
        {
            var app = new UrlGenerator("/");
            var url = app.GetAssetRouteUrl();
            url.ShouldEqual("_assets/get/{*path}");
        }

        [Fact]
        public void GetImageRouteUrl_ReturnsRouteUrlWithPathParameter()
        {
            var app = new UrlGenerator("/");
            var url = app.GetRawFileRouteUrl();
            url.ShouldEqual("_assets/file/{*path}");
        }
    }
}