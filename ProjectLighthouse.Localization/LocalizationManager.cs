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

        string resourceBasename = $"{namespaceStr}.{translationArea.ToString()}";

        // We don't have an en-US .resx, so if we aren't using en-US then we need to add the appropriate language.
        // Otherwise, keep it to the normal .resx file
        // e.g. BaseLayout.resx as opposed to BaseLayout.lang-da-DK.resx.
        if (language != defaultLang) resourceBasename += $".lang-{language}";

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

    // This is a bit scuffed, but it will work for what I need it to do.
    public static IEnumerable<string> GetAvailableLanguages()
    {
        string area = TranslationAreas.BaseLayout.ToString();

        List<string> languages = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .Where(r => r.StartsWith($"{namespaceStr}.{area}"))
            .Select(r => r.Substring(r.IndexOf(area), r.Length - r.IndexOf(area)).Substring(area.Length + 1))
            .Select(r => r.Replace(".resources", string.Empty)) // Remove .resources
            .Select(r => r.Replace("lang-", string.Empty)) // Remove 'lang-' prefix from languages
            .Where(r => r != "resources")
            .ToList();

        languages.Add(defaultLang);

        return languages;
    }
}