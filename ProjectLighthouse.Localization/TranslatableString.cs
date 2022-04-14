namespace LBPUnion.ProjectLighthouse.Localization;

public class TranslatableString
{
    public TranslatableString(TranslationAreas area, string key)
    {
        this.Key = key;
        this.Area = area;
    }

    public string Key { get; init; }
    public TranslationAreas Area { get; init; }

    public string Translate(string language) => LocalizationManager.GetLocalizedString(this.Area, language, this.Key);

    [Obsolete("Do not translate by using ToString. Use TranslatableString.Translate().", true)]
    public override string ToString() => "NOT TRANSLATED CORRECTLY!";

    [Obsolete("Do not translate by using ToString. Use TranslatableString.Translate().", true)]
    public static implicit operator string(TranslatableString _) => "NOT TRANSLATED CORRECTLY!";
}