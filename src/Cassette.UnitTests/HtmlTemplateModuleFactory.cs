﻿using Should;
using Xunit;
using Moq;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateModuleFactory_Test
    {
        [Fact]
        public void CreateModule_ReturnsHtmlTemplateModuleWithDirectorySet()
        {
            var factory = new HtmlTemplateModuleFactory();
            var module = factory.CreateModule("test");
            module.Directory.ShouldEqual("test");
        }
    }
}
