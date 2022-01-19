using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LBPUnion.ProjectLighthouse.Serialization;

/// <summary>
///     LBP doesn't like the XML serializer by C# that much, and it cant be controlled that much (cant have two root
///     elements),
///     so I wrote my own crappy one.
/// </summary>
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class LbpSerializer
{
    public static string BlankElement(string key) => $"<{key}></{key}>";

    public static string StringElement(KeyValuePair<string, object> pair) => $"<{pair.Key}>{pair.Value}</{pair.Key}>";

    public static string StringElement(string key, bool value) => $"<{key}>{value.ToString().ToLower()}</{key}>";

    public static string StringElement(string key, object value) => $"<{key}>{value}</{key}>";

    public static string TaggedStringElement
        (KeyValuePair<string, object> pair, KeyValuePair<string, object> tagPair)
        => $"<{pair.Key} {tagPair.Key}=\"{tagPair.Value}\">{pair.Value}</{pair.Key}>";

    public static string TaggedStringElement(string key, object value, string tagKey, object tagValue) => $"<{key} {tagKey}=\"{tagValue}\">{value}</{key}>";

    public static string TaggedStringElement(string key, object value, Dictionary<string, object> attrKeyValuePairs)
        => $"<{key} " + attrKeyValuePairs.Aggregate(string.Empty, (current, kvp) => current + $"{kvp.Key}=\"{kvp.Value}\" ") + $">{value}</{key}>";

    public static string Elements
        (params KeyValuePair<string, object>[] pairs)
        => pairs.Aggregate(string.Empty, (current, pair) => current + StringElement(pair));
}