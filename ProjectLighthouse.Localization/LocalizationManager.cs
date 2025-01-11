using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace LBPUnion.ProjectLighthouse.Localization;

public static class LocalizationManager
{
    private static readonly string namespaceStr = typeof(LocalizationManager).Namespace ?? "";
    public const string DefaultLang = "en";

    public static string GetLocalizedString(TranslationAreas translationArea, string language, string key)
    {
        // ASP.NET requires specific names for certain languages (like ja for japanese as opposed to the standard ja-JP)
        // We map that value back here.
        language = mapLanguageBack(language);

        #if DEBUG
        Console.WriteLine($@"Attempting to load '{key}' for '{language}'");
        #endif

        string resourceBasename = $"{namespaceStr}.{translationArea.ToString()}";

        // We don't have an en .resx, so if we aren't using en then we need to add the appropriate language.
        // Otherwise, keep it to the normal .resx file
        // e.g. BaseLayout.resx as opposed to BaseLayout.lang-da-DK.resx.
        if (language != DefaultLang) resourceBasename += $".lang-{language}";

        string? localizedString = null;
        try
        {
            ResourceManager resourceManager = new(resourceBasename,
                Assembly.GetExecutingAssembly());

            localizedString = resourceManager.GetString(key);
        }
        catch
        {
            // ignored
        }
        
        if (localizedString == null)
        {
            #if DEBUG
            if (Debugger.IsAttached) Debugger.Break();
            #endif
            return $"{translationArea.ToString()}.{language}.{key}";
        }

        return localizedString.Replace("\\n", "\n");
    }

    // If a language isn't working, it might be because a language is using a different name than what ASP.NET expects.
    // You can retrieve the name of the language from the <c>Accept-Language</c> header in an HTTP request.
    private static readonly Dictionary<string, string> languageMappings = new()
    {
        {
            "ja-JP", "ja"
        },
        {
            "eo-UY", "eo"
        },
        {
            "ru-RU", "ru"
        },
        {
            "pt-PT", "pt"
        },
        {
            "no-NO", "no"
        },
        {
            "pl-PL", "pl"
        },
        {
            "fr-FR", "fr"
        },
        {
            "de-DE", "de"
        },
        {
            "da-DK", "da"
        },
    };

    /// <summary>
    /// Returns a Crowdin friendly language code from an ASP.NET language code
    /// </summary>
    /// <param name="language">The ASP.NET language code</param>
    /// <returns>The Crowdin friendly language code</returns>
    private static string mapLanguageBack(string language)
        => languageMappings.FirstOrDefault(kv => kv.Value == language, KeyValuePair.Create(language, "")).Key;
    

    private static string getLanguageDisplay(string langCode)
    {
        return langCode switch
        {
            "en-PT" => "Pirate Speak (The Seven Seas)",
            "en-UD" => "English (Upside Down)",
            "zh-CN" => "Simplified Chinese",
            "zh-TW" => "Traditional Chinese",
            "ingsoc" => "Newspeak",
            "toki" => "Toki Pona",
            _ => langCode,
        };
        
    }

    public static string GetLanguageName(string langCode)
    {
        string mappedLanguage = getLanguageDisplay(langCode);
        if(mappedLanguage == langCode && TryGetCultureInfo(langCode, out CultureInfo? info))
        {
            return info!.DisplayName;
        }
        return mappedLanguage;
    }

    public static bool TryGetCultureInfo(string name, out CultureInfo? culture)
    {
        try
        {
            culture = new CultureInfo(name);
            return true;
        }
        catch
        {
            culture = null;
            return false;
        }
    }

    // This is a bit scuffed, but it will work for what I need it to do.
    public static IEnumerable<string> GetAvailableLanguages()
    {
        string area = TranslationAreas.BaseLayout.ToString();

        List<string> languages = Assembly.GetExecutingAssembly()
            .GetManifestResourceNames()
            .Where(r => r.StartsWith($"{namespaceStr}.{area}"))
            .Select(r => r.Substring(r.IndexOf(area), r.Length - r.IndexOf(area))[(area.Length + 1)..])
            .Select(r => r.Replace(".resources", string.Empty)) // Remove .resources
            .Select(r => r.Replace("lang-", string.Empty)) // Remove 'lang-' prefix from languages
            .Where(r => r != "resources")
            .ToList();

        languages.Insert(0, DefaultLang);

        return languages;
    }
}
