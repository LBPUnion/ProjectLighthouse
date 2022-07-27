using System.Reflection;

namespace LBPUnion.ProjectLighthouse.Localization;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("Resource files loaded:");
        foreach (string resourceFile in Assembly.GetExecutingAssembly().GetManifestResourceNames())
        {
            Console.WriteLine("  " + resourceFile);
        }

        Console.Write('\n');

        foreach (string language in LocalizationManager.GetAvailableLanguages())
        {
            Console.WriteLine(LocalizationManager.GetLocalizedString(TranslationAreas.BaseLayout, language, "header_home"));
        }
    }
}