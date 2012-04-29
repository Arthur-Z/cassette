using System.Xml.Linq;

namespace Cassette.Stylesheets.Manifests
{
    class ExternalStylesheetBundleSerializer : StylesheetBundleSerializerBase<ExternalStylesheetBundle>
    {
        public ExternalStylesheetBundleSerializer(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            var element = base.CreateElement();
            element.Add(new XAttribute("Url", Bundle.Url));

            return element;
        }
    }
}