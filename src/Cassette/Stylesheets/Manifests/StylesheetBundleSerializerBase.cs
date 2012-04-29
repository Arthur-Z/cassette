using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleSerializerBase<T> : BundleSerializer<T>
        where T : StylesheetBundle
    {
        protected StylesheetBundleSerializerBase(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            var element = base.CreateElement();
            if (Bundle.Condition != null)
            {
                element.Add(new XAttribute("Condition", Bundle.Condition));
            }
            if (Bundle.Media != null)
            {
                element.Add(new XAttribute("Media", Bundle.Media));
            }
            return element;
        }
    }
}