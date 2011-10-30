using System;
using System.Web;

namespace Cassette
{
    /// <summary>
    /// A do-nothing implementation of <see cref="IPlaceholderTracker"/>.
    /// </summary>
    class NullPlaceholderTracker : IPlaceholderTracker
    {
        public IHtmlString InsertPlaceholder(Func<IHtmlString> futureHtml)
        {
            return futureHtml();
        }

        public string ReplacePlaceholders(string html)
        {
            return html;
        }
    }
}