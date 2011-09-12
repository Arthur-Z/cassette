using System;
using System.Web;

namespace Cassette.Scripts
{
    public class InlineScriptModule : Module
    {
        readonly string scriptContent;

        public InlineScriptModule(string scriptContent)
        {
            this.scriptContent = scriptContent;
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            return new HtmlString(
                "<script type=\"text/javascript\">" + Environment.NewLine + 
                scriptContent + Environment.NewLine + 
                "</script>"
                );
        }
    }
}