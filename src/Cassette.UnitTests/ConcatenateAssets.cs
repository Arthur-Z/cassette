﻿using System;
using System.Linq;
using System.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ConcatenateAssets_Tests
    {
        [Fact]
        public void GivenModuleWithTwoAssets_WhenConcatenateAssetsProcessesModule_ThenASingleAssetReplacesTheTwoOriginalAssets()
        {
            var module = new Module("c:\\");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.OpenStream()).Returns(() => "asset1".AsStream());
            asset2.Setup(a => a.OpenStream()).Returns(() => "asset2".AsStream());
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            var processor = new ConcatentateAssets();
            processor.Process(module);

            module.Assets.Count.ShouldEqual(1);
            using (var reader = new StreamReader(module.Assets[0].OpenStream()))
            {
                reader.ReadToEnd().ShouldEqual("asset1" + Environment.NewLine + "asset2");
            }
            (module.Assets[0] as IDisposable).Dispose();
        }

        [Fact]
        public void ConcatenateAssetsMergesAssetReferences()
        {
            var module = new Module("c:\\");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.OpenStream()).Returns(() => "asset1".AsStream());
            asset1.SetupGet(a => a.References).Returns(new[] { new AssetReference("c:\\other1.js", asset1.Object, 0, AssetReferenceType.DifferentModule) });
            asset2.Setup(a => a.OpenStream()).Returns(() => "asset2".AsStream());
            asset2.SetupGet(a => a.References).Returns(new[] { new AssetReference("c:\\other1.js", asset2.Object, 0, AssetReferenceType.DifferentModule) });
            asset2.SetupGet(a => a.References).Returns(new[] { new AssetReference("c:\\other2.js", asset2.Object, 0, AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            var processor = new ConcatentateAssets();
            processor.Process(module);

            module.Assets[0].References
                .Select(r => r.ReferencedFilename)
                .OrderBy(f => f)
                .SequenceEqual(new[] { "c:\\other1.js", "c:\\other2.js" })
                .ShouldBeTrue();
        }
    }
}
