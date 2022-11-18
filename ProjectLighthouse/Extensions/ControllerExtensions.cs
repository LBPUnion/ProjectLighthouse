#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class ControllerExtensions
{

    public static GameToken GetToken(this ControllerBase controller)
    {
        GameToken? token = (GameToken?)(controller.HttpContext.Items["Token"] ?? null);
        if (token == null) throw new ArgumentNullException($"GameToken was null even though authentication was successful {nameof(controller)}");

        return token;
    }

    public static async Task<T?> DeserializeBody<T>(this ControllerBase controller, params string[] rootElements)
    {
        controller.Request.Body.Position = 0;
        string bodyString = await new StreamReader(controller.Request.Body).ReadToEndAsync();

        try
        {
            // Prevent unescaped ampersands from causing deserialization to fail
            bodyString = Regex.Replace(bodyString, "&(?!(amp|apos|quot|lt|gt);)", "&amp;");

            XmlRootAttribute? root = null;
            if (rootElements.Length > 0)
            {
                //TODO: This doesn't support root tags with attributes, but it's only used in scenarios where there shouldn't any (UpdateUser and Playlists)
                string? matchedRoot = rootElements.FirstOrDefault(e => bodyString.StartsWith($@"<{e}>"));
                if (matchedRoot == null)
                {
                    Logger.Error($"[{controller.ControllerContext.ActionDescriptor.ActionName}] " +
                                 $"Failed to deserialize {typeof(T).Name}: Unable to match root element", LogArea.Deserialization);
                    Logger.Error($"{bodyString}", LogArea.Deserialization);
                    return default;
                }
                root = new XmlRootAttribute(matchedRoot);
            }
            XmlSerializer serializer = new(typeof(T), root);
            T? obj = (T?)serializer.Deserialize(new StringReader(bodyString));
            SanitizationHelper.SanitizeStringsInClass(obj);
            return obj;
        }
        catch (Exception e)
        {
            Logger.Error($"[{controller.ControllerContext.ActionDescriptor.ActionName}] " +
                         $"Failed to deserialize {typeof(T).Name}: {e.Message}", LogArea.Deserialization);
            Logger.Error($"{bodyString}", LogArea.Deserialization);
        }
        return default;
    }
}