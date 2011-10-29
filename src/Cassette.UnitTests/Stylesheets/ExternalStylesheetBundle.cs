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

using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetBundle_Tests
    {
        [Fact]
        public void CanCreateNamedBundle()
        {
            var bundle = new ExternalStylesheetBundle("http://url.com/", "~/name");
            bundle.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateNamedBundle_ThenPathIsAppRelative()
        {
            var bundle = new ExternalStylesheetBundle("http://url.com/", "name");
            bundle.Path.ShouldEqual("~/name");
        }

        [Fact]
        public void CreateWithOnlyUrl_ThenPathIsUrl()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/api.css");
            bundle.Path.ShouldEqual("http://test.com/api.css");
        }

        [Fact]
        public void RenderUsesRenderer()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css");
            var application = new Mock<ICassetteApplication>();
            var urlGenerator = new Mock<IUrlGenerator>();
            application.SetupGet(a => a.UrlGenerator).Returns(urlGenerator.Object);
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("/");
            bundle.Process(application.Object);

            var html = bundle.Render();

            html.ToHtmlString().ShouldContain(bundle.Url);
        }

        [Fact]
        public void GivenMediaNotEmpty_RenderReturnsHtmlLinkElementWithMediaAttribute()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css")
            {
                Media = "print"
            };
            var application = new Mock<ICassetteApplication>();
            var urlGenerator = new Mock<IUrlGenerator>();
            application.SetupGet(a => a.UrlGenerator).Returns(urlGenerator.Object);
            urlGenerator.Setup(g => g.CreateBundleUrl(bundle)).Returns("/");
            bundle.Process(application.Object);

            var html = bundle.Render();

            html.ToHtmlString().ShouldContain(bundle.Url);
            html.ToHtmlString().ShouldContain("media=\"print\"");
        }

        [Fact]
        public void ProcessCallsProcessor()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css");
            var processor = new Mock<IBundleProcessor<StylesheetBundle>>();
            bundle.Processor = processor.Object;

            bundle.Process(Mock.Of<ICassetteApplication>());

            processor.Verify(p => p.Process(bundle, It.IsAny<ICassetteApplication>()));
        }

        [Fact]
        public void GivenBundleHasName_WhenContainsPathUrl_ThenReturnTrue()
        {
            var bundle = new ExternalStylesheetBundle("http://test.com/asset.css", "test");
            bundle.ContainsPath("http://test.com/asset.css").ShouldBeTrue();
        }
    }
}