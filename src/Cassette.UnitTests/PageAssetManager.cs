﻿using System;
using System.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class PageAssetManager_Tests
    {
        public PageAssetManager_Tests()
        {
            referenceBuilder = new Mock<IReferenceBuilder<Module>>();
            manager = new PageAssetManager<Module>(referenceBuilder.Object, Mock.Of<ICassetteApplication>());
        }

        readonly PageAssetManager<Module> manager;
        readonly Mock<IReferenceBuilder<Module>> referenceBuilder;

        [Fact]
        public void WhenAddReference_ThenReferenceBuilderIsCalled()
        {
            manager.AddReference("test");
            referenceBuilder.Verify(b => b.AddReference("test"));
        }

        [Fact]
        public void GivenAddReferenceToPath_WhenRender_ThenModuleRenderOutputReturned()
        {
            var module = new Mock<Module>("stub", Mock.Of<IFileSystem>());
            module.Setup(m => m.Render(It.IsAny<ICassetteApplication>()))
                  .Returns(new HtmlString("output"));
            referenceBuilder.Setup(b => b.GetModules()).Returns(new[] { module.Object });
            manager.AddReference("test");

            var html = manager.Render();

            html.ToHtmlString().ShouldEqual("output");
        }

        [Fact]
        public void GivenAddReferenceToTwoPaths_WhenRender_ThenModuleRenderOutputsSeparatedByNewLinesReturned()
        {
            var module1 = new Mock<Module>("stub1", Mock.Of<IFileSystem>());
            module1.Setup(m => m.Render(It.IsAny<ICassetteApplication>()))
                  .Returns(new HtmlString("output1"));
            var module2 = new Mock<Module>("stub2", Mock.Of<IFileSystem>());
            module2.Setup(m => m.Render(It.IsAny<ICassetteApplication>()))
                   .Returns(new HtmlString("output2"));
            referenceBuilder.Setup(b => b.GetModules())
                            .Returns(new[] { module1.Object, module2.Object });
            manager.AddReference("stub1");
            manager.AddReference("stub2");

            var html = manager.Render();

            html.ToHtmlString().ShouldEqual("output1" + Environment.NewLine + "output2");
        }
    }
}
