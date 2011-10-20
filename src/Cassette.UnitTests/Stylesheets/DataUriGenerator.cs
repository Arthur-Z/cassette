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

using System;
using System.IO;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class DataUriGenerator_Tests
    {
        public DataUriGenerator_Tests()
        {
            transformer = new DataUriGenerator();

            directory = new Mock<IDirectory>();
            asset = new Mock<IAsset>();
            var file = new Mock<IFile>();
            asset.SetupGet(a => a.SourceFile.FullPath)
                 .Returns("asset.css");
            asset.SetupGet(a => a.SourceFile)
                 .Returns(file.Object);
            file.SetupGet(f => f.Directory)
                .Returns(directory.Object);
        }

        readonly Mock<IAsset> asset;
        readonly Mock<IDirectory> directory;
        readonly DataUriGenerator transformer;

        [Fact]
        public void TransformReplacesImageUrlWithDataUri()
        {
            StubFile("test.png", new byte[] { 1, 2, 3 });
            
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            var expectedBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(data:image/png;base64," + expectedBase64 + "); }"
            );
        }

        [Fact]
        public void ImageUrlCanHaveSubDirectory()
        {
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/styles/jquery-ui/jquery-ui.css");
            asset.SetupGet(a => a.SourceFile.Directory).Returns(directory.Object);
            StubFile("images/test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            var expectedBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(data:image/png;base64," + expectedBase64 + "); }"
            );
        }

        [Fact]
        public void FileWithJpgExtensionCreatesImageJpegDataUri()
        {
            StubFile("test.jpg", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.jpg); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldContain("image/jpeg");
        }

        [Fact]
        public void AssetAddRawFileReferenceIsCalled()
        {
            StubFile("test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            getResult();

            asset.Verify(a => a.AddRawFileReference("test.png"));
        }

        [Fact]
        public void GivenFileDoesNotExists_WhenTransform_ThenUrlIsNotChanged()
        {
            var file = new Mock<IFile>();
            file.SetupGet(f => f.Exists).Returns(false);
            directory.Setup(d => d.GetFile(It.IsAny<string>()))
                     .Returns(file.Object);

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png); }"
            );
        }

        void StubFile(string filename, byte[] bytes)
        {
            var file = new Mock<IFile>();
            directory.Setup(d => d.GetFile(filename))
                .Returns(file.Object);
            file.SetupGet(f => f.Directory)
                .Returns(directory.Object);
            file.SetupGet(f => f.Exists)
                .Returns(true);
            file.Setup(d => d.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(() => new MemoryStream(bytes));
        }

        // TODO: Add legacy IE support for data-uris.
        // This is hard because we need to add a css star-hack property after the property with the image.
        // e.g. p { background: #fff url(blah.png); }
        //   -> p { background: #fff url(data:image/png;base64,xxxx); *background-image: url(mhtml:http://test.com/this.css!image0); }
        // So the transformer must understand how to expand compact CSS rules into "-image" versions.

        //[Fact]
        //public void GivenLegacyIESupport_ThenTransformConvertsCssToMultipartDocument()
        //{
        //    var asset = new Mock<IAsset>();
        //    asset.SetupGet(a => a.Path)
        //         .Returns(directory.Object);
        //    directory.Setup(d => d.OpenFile("test.png", FileMode.Open, FileAccess.Read))
        //             .Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));

        //    transformer.LegacyIESupport = true;
        //    var css = "p { background-image: url(test.png); }";
        //    var getResult = transformer.Transform(css.AsStream, asset.Object);

        //    var expectedBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        //    getResult().ReadToEnd().ShouldEqual(
        //        "/*" +
        //        "\r\n" + 
        //        "Content-Type: multipart/related; boundary=\"CASSETTE-IMAGE-BOUNDARY\"" +
        //        "\r\n\r\n" + 
        //        "--CASSETTE-IMAGE-BOUNDARY"+
        //        "\r\n" + 
        //        "Content-Location: i0" +
        //        "\r\n" +
        //        "Content-Transfer-Encoding: base64" +
        //        "\r\n\r\n" + 
        //        expectedBase64 +
        //        "\r\n\r\n" + 
        //        "--CASSETTE-IMAGE-BOUNDARY--" +
        //        "\r\n" + 
        //        "*/" +
        //        "\r\n" + 
        //        "p { background-image: url(data:image/png;base64," + expectedBase64 + "); *background-image:url(mhtml:http://test.com/styles.css!i0); }"
        //    );
        //}
        
    }
}

