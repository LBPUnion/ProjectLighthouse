using System.Diagnostics;
using System.Reflection;
using System.Resources;

namespace LBPUnion.ProjectLighthouse.Localization;

public static class LocalizationManager
{
    private static readonly string namespaceStr = typeof(LocalizationManager).Namespace ?? "";
    private const string defaultLang = "en-US";

    public static string GetLocalizedString(TranslationAreas translationArea, string language, string key)
    {
        #if DEBUG
        Console.WriteLine($"Attempting to load '{key}' for '{language}'");
        #endif

        string resourceBasename;
        if (language == defaultLang)
        {
            resourceBasename = $"{namespaceStr}.{translationArea.ToString()}";
        }
        else
        {
            resourceBasename = $"{namespaceStr}.{translationArea.ToString()}.lang-{language}";
        }

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
        string area = translationArea.ToString();

        // scuffed but it will work for now
        List<string> langs = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .Where(r => r.StartsWith($"{namespaceStr}.{area}"))
            .Select(r => r.Substring(r.IndexOf(area), r.Length - r.IndexOf(area)).Substring(area.Length + 1))
            .Select(r => r.Replace(".resources", string.Empty)) // Remove .resources
            .Select(r => r.Replace("lang-", string.Empty)) // Remove 'lang-' prefix from languages
            .Where(r => r != "resources")
            .ToList();

        langs.Add(defaultLang);

        return langs;
    }
}