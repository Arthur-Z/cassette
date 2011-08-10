﻿using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleContainer_Tests
    {
        void SetupAsset(string filename, Mock<IAsset> asset)
        {
            asset.Setup(a => a.SourceFilename).Returns(filename);
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentModuleReference_ThenConstructorThrowsAssetReferenceException()
        {
            var module = new Module("module-1", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("module-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("fail\\fail.js", asset.Object, 0, AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] { module });
            });
            exception.Message.ShouldEqual("Reference error in \"module-1\\a.js\". Cannot find \"fail\\fail.js\".");
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentModuleReferenceHavingLineNumber_ThenConstructorThrowsAssetReferenceException()
        {
            var module = new Module("module-1", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("module-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("fail\\fail.js", asset.Object, 42, AssetReferenceType.DifferentModule) });
            module.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new ModuleContainer<Module>(new[] { module });
            });
            exception.Message.ShouldEqual("Reference error in \"module-1\\a.js\", line 42. Cannot find \"fail\\fail.js\".");
        }

        [Fact]
        public void FindModuleByPathOfModuleReturnsTheModule()
        {
            var expectedModule = new Module("test", Mock.Of<IFileSystem>());
            var container = new ModuleContainer<Module>(new[] {
                expectedModule
            });
            var actualModule = container.FindModuleByPath("test");
            actualModule.ShouldBeSameAs(expectedModule);
        }

        [Fact]
        public void FindModuleByPathWithWrongPathReturnsNull()
        {
            var container = new ModuleContainer<Module>(new[] {
                new Module("test", Mock.Of<IFileSystem>())
            });
            var actualModule = container.FindModuleByPath("WRONG");
            actualModule.ShouldBeNull();
        }

        [Fact]
        public void FindModuleByPathOfAssetReturnsTheModule()
        {
            var expectedModule = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.SourceFilename).Returns("test.js");
            expectedModule.Assets.Add(asset.Object);
            var container = new ModuleContainer<Module>(new[] {
                expectedModule
            });
            var actualModule = container.FindModuleByPath("test/test.js");
            actualModule.ShouldBeSameAs(expectedModule);
        }

        [Fact]
        public void GivenModule1ReferencesModule2_ThenAddDependenciesAndSortModule1ReturnsModule2AndModule1()
        {
            var module1 = new Module("module-1", Mock.Of<IFileSystem>());
            var asset1 = new Mock<IAsset>();
            SetupAsset("a.js", asset1);
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("module-2\\b.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module1.Assets.Add(asset1.Object);

            var module2 = new Module("module-2", Mock.Of<IFileSystem>());
            var asset2 = new Mock<IAsset>();
            SetupAsset("b.js", asset2);
            module2.Assets.Add(asset2.Object);

            var container = new ModuleContainer<Module>(new[] { module1, module2 });

            container.AddDependenciesAndSort(new[] { module1 })
                .SequenceEqual(new[] { module2, module1 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenModule1ReferencesModule2WhichReferencesModule3_ThenAddDependenciesAndSortModule1ReturnsModule3AndModule2AndModule1()
        {
            var module1 = new Module("module-1", Mock.Of<IFileSystem>());
            var asset1 = new Mock<IAsset>();
            SetupAsset("a.js", asset1);
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("module-2\\b.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module1.Assets.Add(asset1.Object);

            var module2 = new Module("module-2", Mock.Of<IFileSystem>());
            var asset2 = new Mock<IAsset>();
            SetupAsset("b.js", asset2);
            asset2.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("module-3\\c.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module2.Assets.Add(asset2.Object);

            var module3 = new Module("module-3", Mock.Of<IFileSystem>());
            var asset3 = new Mock<IAsset>();
            SetupAsset("c.js", asset3);
            module3.Assets.Add(asset3.Object);

            var container = new ModuleContainer<Module>(new[] { module1, module2, module3 });

            container.AddDependenciesAndSort(new[] { module1 })
                .SequenceEqual(new[] { module3, module2, module1 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenDiamondReferencing_ThenAddDependenciesAndSortReturnsEachReferencedModuleOnlyOnceInDependencyOrder()
        {
            var module1 = new Module("module-1", Mock.Of<IFileSystem>());
            var asset1 = new Mock<IAsset>();
            SetupAsset("a.js", asset1);
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { 
                      new AssetReference("module-2\\b.js", asset1.Object, 1, AssetReferenceType.DifferentModule),
                      new AssetReference("module-3\\c.js", asset1.Object, 1, AssetReferenceType.DifferentModule)
                  });
            module1.Assets.Add(asset1.Object);

            var module2 = new Module("module-2", Mock.Of<IFileSystem>());
            var asset2 = new Mock<IAsset>();
            SetupAsset("b.js", asset2);
            asset2.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("module-4\\d.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module2.Assets.Add(asset2.Object);

            var module3 = new Module("module-3", Mock.Of<IFileSystem>());
            var asset3 = new Mock<IAsset>();
            SetupAsset("c.js", asset3);
            asset3.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("module-4\\d.js", asset1.Object, 1, AssetReferenceType.DifferentModule) });
            module3.Assets.Add(asset3.Object);
            
            var module4 = new Module("module-4", Mock.Of<IFileSystem>());
            var asset4 = new Mock<IAsset>();
            SetupAsset("d.js", asset4);
            module4.Assets.Add(asset4.Object);

            var container = new ModuleContainer<Module>(new[] { module1, module2, module3, module4 });

            container.AddDependenciesAndSort(new[] { module1 })
                .SequenceEqual(new[] { module4, module2, module3, module1 }).ShouldBeTrue();
        }
    }
}
