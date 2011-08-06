﻿using System;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class Module_Tests
    {
        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingBackSlash()
        {
            var module = new Module("c:\\test\\");
            module.Directory.ShouldEqual("c:\\test");
        }

        [Fact]
        public void ConstructorNormalizesDirectoryPathByRemovingTrailingForwardSlash()
        {
            var module = new Module("c:\\test/");
            module.Directory.ShouldEqual("c:\\test");
        }

        [Fact]
        public void ContainsPathOfAssetInModule_ReturnsTrue()
        {
            var module = new Module("c:\\test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.IsFrom("c:\\test\\asset.js")).Returns(true);
            module.Assets.Add(asset.Object);

            module.ContainsPath("c:\\test\\asset.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetInModuleWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("c:\\test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.IsFrom("c:\\TEST\\ASSET.js")).Returns(true);
            module.Assets.Add(asset.Object);

            module.ContainsPath("c:\\TEST\\ASSET.js").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfAssetNotInModule_ReturnsFalse()
        {
            var module = new Module("c:\\test");

            module.ContainsPath("c:\\test\\no-in-module.js").ShouldBeFalse();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItself_ReturnsTrue()
        {
            var module = new Module("c:\\test");

            module.ContainsPath("c:\\test").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithDifferentCasing_ReturnsTrue()
        {
            var module = new Module("c:\\test");

            module.ContainsPath("c:\\TEST").ShouldBeTrue();
        }

        [Fact]
        public void ContainsPathOfJustTheModuleItselfWithTrailingSlash_ReturnsTrue()
        {
            var module = new Module("c:\\test");

            module.ContainsPath("c:\\test\\").ShouldBeTrue();
        }

        [Fact]
        public void DisposeDisposesAllDisposableAssets()
        {
            var module = new Module("c:\\");
            var asset1 = new Mock<IDisposable>();
            var asset2 = new Mock<IDisposable>();
            var asset3 = new Mock<IAsset>(); // Not disposable; Tests for incorrect casting to IDisposable.
            module.Assets.Add(asset1.As<IAsset>().Object);
            module.Assets.Add(asset2.As<IAsset>().Object);
            module.Assets.Add(asset3.Object);

            module.Dispose();

            asset1.Verify(a => a.Dispose());
            asset2.Verify(a => a.Dispose());
        }
    }
}
