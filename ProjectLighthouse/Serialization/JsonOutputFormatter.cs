using Microsoft.AspNetCore.Mvc.Formatters;

namespace LBPUnion.ProjectLighthouse.Serialization;

public class JsonOutputFormatter : StringOutputFormatter
{
    public JsonOutputFormatter()
    {
        this.SupportedMediaTypes.Add("text/json");
        this.SupportedMediaTypes.Add("application/json");
    }
}