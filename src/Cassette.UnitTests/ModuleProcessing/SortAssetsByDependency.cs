﻿using System;
using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class SortAssetsByDependency_Tests
    {
        [Fact]
        public void GivenTwoAssetsWhereADependsOnB_WhenSorted_ThenBIsBeforeAInModule()
        {
            var module = new Module("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFilename).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/b.js", assetA.Object, 1, AssetReferenceType.SameModule) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFilename).Returns("~/test/b.js");
            module.Assets.Add(assetA.Object);
            module.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(module, Mock.Of<ICassetteApplication>());

            module.Assets[0].ShouldBeSameAs(assetB.Object);
            module.Assets[1].ShouldBeSameAs(assetA.Object);
        }

        [Fact]
        public void GivenTwoAssetsWhereADependsOnBByDifferentlyCasedFilename_WhenSorted_ThenBIsBeforeAInModule()
        {
            var module = new Module("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFilename).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/TEST/B.js", assetA.Object, 1, AssetReferenceType.SameModule) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFilename).Returns("~/test/b.js");
            module.Assets.Add(assetA.Object);
            module.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(module, Mock.Of<ICassetteApplication>());

            module.Assets[0].ShouldBeSameAs(assetB.Object);
            module.Assets[1].ShouldBeSameAs(assetA.Object);
        }

        [Fact]
        public void WhenModuleHasSortedAssets_ThenProcessDoesNotReorderAssets()
        {
            var module = new Module("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFilename).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/TEST/B.js", assetA.Object, 1, AssetReferenceType.SameModule) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFilename).Returns("~/test/b.js");
            module.AddAssets(new[] {assetA.Object, assetB.Object}, preSorted: true);
            
            var sorter = new SortAssetsByDependency();
            sorter.Process(module, Mock.Of<ICassetteApplication>());

            module.Assets[0].ShouldBeSameAs(assetA.Object);
            module.Assets[1].ShouldBeSameAs(assetB.Object);
        }

        [Fact]
        public void GivenAssetWithCircularReferences_WhenProcess_ThenExceptionThrown()
        {
            var module = new Module("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFilename).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/b.js", assetA.Object, 1, AssetReferenceType.SameModule) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFilename).Returns("~/test/b.js");
            assetB.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/a.js", assetB.Object, 1, AssetReferenceType.SameModule) });

            module.AddAssets(new[] { assetA.Object, assetB.Object }, preSorted: false);

            var sorter = new SortAssetsByDependency();
            Assert.Throws<InvalidOperationException>(
                () => sorter.Process(module, Mock.Of<ICassetteApplication>())
            );
        }
    }
}
