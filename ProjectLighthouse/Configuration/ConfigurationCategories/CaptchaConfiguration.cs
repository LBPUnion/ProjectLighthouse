namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class CaptchaConfiguration
{
    public bool CaptchaEnabled { get; set; }

    public CaptchaType Type { get; set; } = CaptchaType.HCaptcha;

    public string SiteKey { get; set; } = "";

    public string Secret { get; set; } = "";
}