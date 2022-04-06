#nullable enable
using System.Reflection;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class ReflectionHelper
{

    public static void sanitizeStringsInClass(object? instance)
    {
        if (instance == null) return;
        PropertyInfo[] properties = instance.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            if (property.PropertyType != typeof(string)) continue;

            string? before = (string?) property.GetValue(instance);
            if (before == null) continue;
            if (!before.Contains('>') || !before.Contains('<')) continue;
            
            property.SetValue(instance, before.Replace("<", "&lt;").Replace(">", "&gt;"));
        }
    }
    
}