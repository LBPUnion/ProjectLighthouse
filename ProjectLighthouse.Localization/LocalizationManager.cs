using System.Diagnostics;
using System.Reflection;
using System.Resources;

namespace LBPUnion.ProjectLighthouse.Localization;

public static class LocalizationManager
{
    private static readonly string namespaceStr = typeof(LocalizationManager).Namespace ?? "";

    public static string GetLocalizedString(TranslationAreas translationArea, string language, string key)
    {
        #if DEBUG
        Console.WriteLine($"Attempting to load '{key}' for '{language}' ");
        #endif

        string resourceBasename = $"{namespaceStr}.{translationArea.ToString()}.{language}";
        ResourceManager resourceManager = new(resourceBasename, Assembly.GetExecutingAssembly());

        string? localizedString = resourceManager.GetString(key);
        if (localizedString == null)
        {
            #if DEBUG
            if (Debugger.IsAttached) Debugger.Break();
            #endif
            return $"{translationArea.ToString()}.{language}.{key}";
        }

        return localizedString;
    }

    public static IEnumerable<string> GetAvailableLanguages(TranslationAreas translationArea)
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(r => r.StartsWith($"{namespaceStr}.{translationArea.ToString()}"));
    }
}