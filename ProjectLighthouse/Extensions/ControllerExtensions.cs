#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static partial class ControllerExtensions
{

    public static GameTokenEntity GetToken(this ControllerBase controller)
    {
        GameTokenEntity? token = (GameTokenEntity?)(controller.HttpContext.Items["Token"] ?? null);
        if (token == null) throw new ArgumentNullException(nameof(controller), @"GameToken was null even though authentication was successful");

        return token;
    }

    public static async Task<string> ReadBodyAsync(this ControllerBase controller)
    {
        byte[] bodyBytes = await controller.Request.BodyReader.ReadAllAsync();
        if (controller.Request.ContentLength != null && bodyBytes.Length != controller.Request.ContentLength)
        {
            Logger.Warn($"Failed to read entire body, contentType={controller.Request.ContentType}, " +
                        $"contentLen={controller.Request.ContentLength}, readLen={bodyBytes.Length}",
                LogArea.HTTP);
        }
        return Encoding.UTF8.GetString(bodyBytes);
    }

    [GeneratedRegex("&(?!(amp|apos|quot|lt|gt);)")]
    private static partial Regex CharacterEscapeRegex();

    public static async Task<T?> DeserializeBody<T>(this ControllerBase controller, params string[] rootElements)
    {
        string bodyString = await controller.ReadBodyAsync();
        try
        {
            // Prevent unescaped ampersands from causing deserialization to fail
            bodyString = CharacterEscapeRegex().Replace(bodyString, "&amp;");

            XmlRootAttribute? root = null;
            if (rootElements.Length > 0)
            {
                XmlDocument doc = new();
                doc.LoadXml(bodyString);
                string? rootElement = doc.DocumentElement?.Name;
                if (rootElement == null || !rootElements.Contains(rootElement))
                {
                    Logger.Error($"[{controller.ControllerContext.ActionDescriptor.ActionName}] " +
                                 $"Failed to deserialize {typeof(T).Name}: Unable to match root element\n" +
                                 $"rootElement: '{rootElement ?? "null"}'" +
                                 $"xmlData: '{bodyString}'",
                        LogArea.Deserialization);
                    return default;
                }
                root = new XmlRootAttribute(rootElement);
            }
            XmlSerializer serializer = LighthouseSerializer.GetSerializer(typeof(T), root);
            T? obj = (T?)serializer.Deserialize(new StringReader(bodyString));
            return obj;
        }
        catch (Exception e)
        {
            Logger.Error($"[{controller.ControllerContext.ActionDescriptor.ActionName}] " +
                         $"Failed to deserialize {typeof(T).Name}:\n" +
                         $"xmlData: '{bodyString}'\n" +
                         $"detailedException: '{e.ToDetailedException()}",
                LogArea.Deserialization);
        }
        return default;
    }
}