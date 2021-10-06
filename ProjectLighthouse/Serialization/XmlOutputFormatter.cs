using Microsoft.AspNetCore.Mvc.Formatters;

namespace ProjectLighthouse.Serialization {
    public class XmlOutputFormatter : StringOutputFormatter {
        public XmlOutputFormatter() {
            SupportedMediaTypes.Add("text/xml");
            SupportedMediaTypes.Add("application/xml");
        }
    }
}