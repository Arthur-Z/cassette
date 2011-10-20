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

using System.IO;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrlsAssetTransformer_Tests
    {
        public ExpandCssUrlsAssetTransformer_Tests()
        {
            directory = new Mock<IDirectory>();
            file = new Mock<IFile>();
            urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(u => u.CreateRawFileUrl(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns<string, string>((f, h) => "EXPANDED");
            directory.SetupGet(d => d.FullPath).Returns("~/styles");
            
            file.SetupGet(f => f.Exists).Returns(true);
            file.SetupGet(f => f.Directory).Returns(directory.Object);
            file.SetupGet(f => f.FullPath).Returns("~/styles/asset.css");
            file.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(Stream.Null);

            transformer = new ExpandCssUrlsAssetTransformer(directory.Object, urlGenerator.Object);
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile).Returns(file.Object);
        }

        readonly ExpandCssUrlsAssetTransformer transformer;
        readonly Mock<IAsset> asset;
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly Mock<IFile> file;
        readonly Mock<IDirectory> directory;

        void SetupDirectoryGetFile(IFile file)
        {
            directory.Setup(d => d.GetFile(It.IsAny<string>()))
                     .Returns(file);
        }

        [Fact]
        public void GivenCssWithRelativeUrl_WhenTransformed_ThenUrlIsExpanded()
        {
            SetupDirectoryGetFile(StubImageFile().Object);
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssUrlFileIsNotFound_WhenTransform_ThenUrlIsNotExpanded()
        {
            var imageFile = StubImageFile();
            imageFile.SetupGet(f => f.Exists).Returns(false);
            SetupDirectoryGetFile(imageFile.Object);

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(test.png); }");
        }

        [Fact]
        public void GivenCssWithUrlWithFragment_WhenTransformed_ThenUrlIsExpanded()
        {
            var imageFile = StubImageFile();
            SetupDirectoryGetFile(imageFile.Object);
            var css = "p { background-image: url(test.png#fragment); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssWithWhitespaceAroundRelativeUrl_WhenTransformed_ThenUrlIsExpanded()
        {
            var imageFile = StubImageFile();
            SetupDirectoryGetFile(imageFile.Object);
            var css = "p { background-image: url(\n test.png \n); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(\n EXPANDED \n); }");
        }

        [Fact]
        public void GivenCssWithDoubleQuotedRelativeUrl_WhenTransformed_ThenUrlIsExpandedWithoutQuotes()
        {
            var imageFile = StubImageFile();
            SetupDirectoryGetFile(imageFile.Object);

            var css = "p { background-image: url(\"test.png\"); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(\"EXPANDED\"); }");
        }

        [Fact]
        public void GivenCssWithSingleQuotedRelativeUrl_WhenTransformed_ThenUrlIsExpandedWithoutQuotes()
        {
            var imageFile = StubImageFile();
            SetupDirectoryGetFile(imageFile.Object);

            var css = "p { background-image: url('test.png'); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url('EXPANDED'); }");
        }

        [Fact]
        public void GivenCssWithHttpUrl_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(http://test.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(http://test.com/test.png); }");
        }

        [Fact]
        public void GivenCssWithProtocolRelativeUrl_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(//test.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(//test.com/test.png); }");
        }

        [Fact]
        public void GivenCssWithDataUri_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(data:image/png;base64,abc); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(data:image/png;base64,abc); }");
        }

        [Fact]
        public void GivenCssWithDataUriInDoubleQuotes_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(\"data:image/png;base64,abc\"); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(\"data:image/png;base64,abc\"); }");
        }

        [Fact]
        public void GivenCssWithDataUriInSingleQuotes_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url('data:image/png;base64,abc'); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url('data:image/png;base64,abc'); }");
        }

        [Fact]
        public void GivenCssWithUrlToDifferentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            SetupDirectoryGetFile(StubImageFile().Object);
            var css = "p { background-image: url(images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/images/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssWithUrlToParentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            var imageFile = StubImageFile();
            SetupDirectoryGetFile(imageFile.Object);
            var css = "p { background-image: url(../images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/images/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenAssetInSubDirectoryAndCssWithUrlToParentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            directory.SetupGet(f => f.FullPath).Returns("~/styles/sub");

            var imageFile = StubImageFile();
            SetupDirectoryGetFile(imageFile.Object);

            var css = "p { background-image: url(../images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/images/test.png", It.IsAny<string>()));
        }

        static Mock<IFile> StubImageFile()
        {
            var imageFile = new Mock<IFile>();
            imageFile.SetupGet(f => f.Exists).Returns(true);
            imageFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
                .Returns(Stream.Null);
            return imageFile;
        }
    }
}