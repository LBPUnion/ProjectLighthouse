#nullable enable
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static partial class ControllerExtensions
{

    public static GameToken GetToken(this ControllerBase controller)
    {
        GameToken? token = (GameToken?)(controller.HttpContext.Items["Token"] ?? null);
        if (token == null) throw new ArgumentNullException(nameof(controller), @"GameToken was null even though authentication was successful");

        return token;
    }

    private static void AddStringToBuilder(StringBuilder builder, in ReadOnlySequence<byte> readOnlySequence)
    {
        // Separate method because Span/ReadOnlySpan cannot be used in async methods
        ReadOnlySpan<byte> span = readOnlySequence.IsSingleSegment
            ? readOnlySequence.First.Span
            : readOnlySequence.ToArray().AsSpan();
        builder.Append(Encoding.UTF8.GetString(span));
    }

    public static async Task<string> ReadBodyAsync(this ControllerBase controller)
    {
        StringBuilder builder = new();

        while (true)
        {
            ReadResult readResult = await controller.Request.BodyReader.ReadAsync();
            ReadOnlySequence<byte> buffer = readResult.Buffer;

            SequencePosition? position;

            do
            {
                // Look for a EOL in the buffer
                position = buffer.PositionOf((byte)'\n');
                if (position == null) continue;

                ReadOnlySequence<byte> readOnlySequence = buffer.Slice(0, position.Value);
                AddStringToBuilder(builder, in readOnlySequence);

                // Skip the line + the \n character (basically position)
                buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
            }
            while (position != null);


            if (readResult.IsCompleted && buffer.Length > 0)
            {
                AddStringToBuilder(builder, in buffer);
            }

            controller.Request.BodyReader.AdvanceTo(buffer.Start, buffer.End);

            // At this point, buffer will be updated to point one byte after the last
            // \n character.
            if (readResult.IsCompleted)
            {
                break;
            }
        }

        return builder.ToString();
    }

    [GeneratedRegex("&(?!(amp|apos|quot|lt|gt);)")]
    private static partial Regex CharacterEscapeRegex();

    public static async Task<T?> DeserializeBody<T>(this ControllerBase controller, params string[] rootElements)
    {
        controller.Request.Body.Position = 0;

        string bodyString = await controller.ReadBodyAsync();

        try
        {
            // Prevent unescaped ampersands from causing deserialization to fail
            bodyString = CharacterEscapeRegex().Replace(bodyString, "&amp;");

            XmlRootAttribute? root = null;
            if (rootElements.Length > 0)
            {
                //TODO: This doesn't support root tags with attributes, but it's only used in scenarios where there shouldn't any (UpdateUser and Playlists)
                string? matchedRoot = rootElements.FirstOrDefault(e => bodyString.StartsWith(@$"<{e}>"));
                if (matchedRoot == null)
                {
                    Logger.Error($"[{controller.ControllerContext.ActionDescriptor.ActionName}] " +
                                 $"Failed to deserialize {typeof(T).Name}: Unable to match root element\n" +
                                 $"xmlData: '{bodyString}'",
                        LogArea.Deserialization);
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
                         $"Failed to deserialize {typeof(T).Name}:\n" +
                         $"xmlData: '{bodyString}'\n" +
                         $"detailedException: '{e.ToDetailedException()}",
                LogArea.Deserialization);
        }
        return default;
    }
}