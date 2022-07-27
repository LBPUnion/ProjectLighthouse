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

    public string Translate(string language, params object?[] format) => string.Format(LocalizationManager.GetLocalizedString(this.Area, language, this.Key), format);

    // CS0809 is a warning about obsolete methods overriding non-obsoleted methods.
    // That works against what we're trying to do here, so we disable the warning here.
    #pragma warning disable CS0809
    [Obsolete("Do not translate by using ToString. Use TranslatableString.Translate().", true)]
    public override string ToString() => "NOT TRANSLATED CORRECTLY!";
    #pragma warning restore CS0809

    [Obsolete("Do not translate by using ToString. Use TranslatableString.Translate().", true)]
    public static implicit operator string(TranslatableString _) => "NOT TRANSLATED CORRECTLY!";
}