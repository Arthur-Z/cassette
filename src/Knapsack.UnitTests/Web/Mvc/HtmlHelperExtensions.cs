﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Should;
using Xunit;

namespace Knapsack.Web.Mvc
{
    // The HtmlHelper extensions are convenience methods over the Knapsack API.
    // They simply delegate to the PageHelper created by KnapsackHttpModule.

    public class HtmlHelperExtensions_tests
    {
        MockPageHelper pageHelper;
        HtmlHelper htmlHelper;

        public HtmlHelperExtensions_tests()
        {
            pageHelper = new MockPageHelper();
            HtmlHelperExtensions.CreatePageHelper = _ => pageHelper;
            var viewContext = new ViewContext();
            htmlHelper = new HtmlHelper(viewContext, new FakeViewDataContainer());
        }

        [Fact]
        public void AddScriptReference_calls_page_helper()
        {
            string calledWithScript = null;
            pageHelper.AddScriptReference = script => calledWithScript = script;

            htmlHelper.AddScriptReference("test.js");

            calledWithScript.ShouldEqual("test.js");
        }

        [Fact]
        public void RenderScripts_calls_page_helper()
        {
            pageHelper.RenderScripts = () => new HtmlString("html");

            var html = htmlHelper.RenderScripts();
            html.ToHtmlString().ShouldEqual("html");
        }

        [Fact]
        public void CreatePageHelperImpl_gets_page_helper_from_HttpContext_items()
        {
            var expectedPageHelper = new MockPageHelper();
            htmlHelper.ViewContext.HttpContext = new FakeHttpContext();
            htmlHelper.ViewContext.HttpContext.Items["Knapsack.PageHelper"] = expectedPageHelper;

            var pageHelper = HtmlHelperExtensions.CreatePageHelperImpl(htmlHelper);
            
            pageHelper.ShouldBeSameAs(expectedPageHelper);
        }

        [Fact]
        public void CreatePageHelperImpl_throws_InvalidOperationException_when_PageHelper_is_not_in_HttpContext_items()
        {
            htmlHelper.ViewContext.HttpContext = new FakeHttpContext();

            Assert.Throws<InvalidOperationException>(delegate 
            {
                HtmlHelperExtensions.CreatePageHelperImpl(htmlHelper); 
            });
        }

        class MockPageHelper : IPageHelper
        {
            public Action<string> AddScriptReference;
            public Func<IHtmlString> RenderScripts;

            void IPageHelper.AddScriptReference(string scriptPath)
            {
                AddScriptReference(scriptPath);
            }

            IHtmlString IPageHelper.RenderScripts()
            {
                return RenderScripts();
            }
        }

        class FakeViewDataContainer : IViewDataContainer
        {
            public ViewDataDictionary ViewData
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }
        }

        class FakeHttpContext : HttpContextBase
        {
            IDictionary items = new Dictionary<string, object>();

            public override IDictionary Items
            {
                get
                {
                    return items;
                }
            }
        }
    }
}
