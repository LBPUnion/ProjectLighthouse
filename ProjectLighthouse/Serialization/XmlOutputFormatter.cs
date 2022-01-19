using Microsoft.AspNetCore.Mvc.Formatters;

namespace LBPUnion.ProjectLighthouse.Serialization;

public class XmlOutputFormatter : StringOutputFormatter
{
    public XmlOutputFormatter()
    {
        this.SupportedMediaTypes.Add("text/xml");
        this.SupportedMediaTypes.Add("application/xml");
    }
}