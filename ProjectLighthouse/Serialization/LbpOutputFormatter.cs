using System;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace LBPUnion.ProjectLighthouse.Serialization;

public class LbpOutputFormatter : TextOutputFormatter
{

    public LbpOutputFormatter()
    {
        this.SupportedMediaTypes.Add("text/xml");

        this.SupportedEncodings.Add(Encoding.UTF8);
        this.SupportedEncodings.Add(Encoding.Unicode);
    }

    protected override bool CanWriteType(Type type)
    {
        bool isSerializable = typeof(ILbpSerializable).IsAssignableFrom(type);
        if (isSerializable) return base.CanWriteType(type);
        Logger.Warn($"Unable to serialize type '{type?.Name}' because it doesn't extend ISerializable: (fullType={type?.FullName}", LogArea.Serialization);
        return false;
    } 

    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        if (context.Object is not ILbpSerializable o) return;

        string serialized = LighthouseSerializer.Serialize(context.HttpContext.RequestServices, o);

        await context.HttpContext.Response.WriteAsync(serialized);
    }
}