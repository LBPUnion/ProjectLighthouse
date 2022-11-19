#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class SanitizationHelper
{

    private static readonly Dictionary<string, string> charsToReplace = new() {
        {"<", "&lt;"},
        {">", "&gt;"},
        {"\"", "&quot;"},
        {"'", "&apos;"},
    };

    public static void SanitizeStringsInClass(object? instance)
    {
        if (instance == null) return;
        PropertyInfo[] properties = instance.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            if (property.PropertyType != typeof(string)) continue;

            string? before = (string?) property.GetValue(instance);
            
            if (before == null) continue;
            if (!charsToReplace.Keys.Any(k => before.Contains(k))) continue;
            
            property.SetValue(instance, SanitizeString(before));
        }
    }

    public static string SanitizeString(string? input)
    {
        if (input == null) return "";

        foreach ((string? key, string? value) in charsToReplace)
        {
            input = input.Replace(key, value);
        }
        return input;
    }
    
}