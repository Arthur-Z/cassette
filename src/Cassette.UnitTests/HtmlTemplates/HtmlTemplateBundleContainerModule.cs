using System;
using System.IO;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundleContainerModule_Tests
    {
        readonly TinyIoCContainer container;
        readonly HtmlTemplateBundleContainerBuilder builder;
        readonly FileSearch fileSearch;

        public HtmlTemplateBundleContainerModule_Tests()
        {
            container = new TinyIoCContainer();
            builder = new HtmlTemplateBundleContainerBuilder(type => new Type[0]);
            builder.Build(container);

            fileSearch = (FileSearch)container.Resolve<IFileSearch>(HostBase.FileSearchComponentName(typeof(HtmlTemplateBundle)));
        }

        [Fact]
        public void FileSearchPatternIsHtmAndHtml()
        {
            fileSearch.Pattern.ShouldContain("*.htm;*.html");
        }

        [Fact]
        public void FileSearchSearchOptionIsAllDirectories()
        {
            fileSearch.SearchOption.ShouldEqual(SearchOption.AllDirectories);
        }

        [Fact]
        public void BundleFactoryIsHtmlTemplateBundleFactory()
        {
            container.Resolve<IBundleFactory<HtmlTemplateBundle>>().ShouldBeType<HtmlTemplateBundleFactory>();
        }

        [Fact]
        public void BundlePipelineIsHtmlTemplatePipeline()
        {
            container.Resolve<IBundlePipeline<HtmlTemplateBundle>>().ShouldBeType<HtmlTemplatePipeline>();
        }
    }
}