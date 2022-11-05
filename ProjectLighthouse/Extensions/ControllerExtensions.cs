#nullable enable
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class ControllerExtensions
{
    public static async Task<T?> DeserializeBody<T>(this ControllerBase controller, params string[]? rootElements)
    {
        controller.Request.Body.Position = 0;
        string bodyString = await new StreamReader(controller.Request.Body).ReadToEndAsync();

        try
        {
            XmlRootAttribute? attr = null;
            if (rootElements != null)
            {
                // This throws an exception if bodyString isn't the right rootElement which is caught by the deserializer
                attr = new XmlRootAttribute(rootElements.First(e => bodyString.StartsWith($@"<{e}>")));
            }
            XmlSerializer serializer = new(typeof(T), attr);
            T? obj = (T?)serializer.Deserialize(new StringReader(bodyString));
            SanitizationHelper.SanitizeStringsInClass(obj);
            return obj;
        }
        catch
        {
            Logger.Error($"Failed to parse {typeof(T).Name}: {bodyString}", LogArea.Deserialization);
        }
        return default;
    }
}