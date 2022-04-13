namespace LBPUnion.ProjectLighthouse.Localization;

public static class Program
{
    public static void Main()
    {
        Console.WriteLine(LocalizationManager.GetLocalizedString(TranslationAreas.BaseLayout, "en-UD", "header_home"));
        Console.WriteLine("Available languages:");
        foreach (string language in LocalizationManager.GetAvailableLanguages(TranslationAreas.BaseLayout))
        {
            Console.WriteLine(language);
        }
    }
}