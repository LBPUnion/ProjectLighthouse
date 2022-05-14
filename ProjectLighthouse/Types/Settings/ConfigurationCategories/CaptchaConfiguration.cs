namespace LBPUnion.ProjectLighthouse.Types.Settings.ConfigurationCategories;

public class CaptchaConfiguration
{
    // TODO: support recaptcha, not just hcaptcha
    // use an enum to define which captcha services can be used?
    // LBPUnion.ProjectLighthouse.Types.Settings.CaptchaService
    public bool CaptchaEnabled { get; set; }

    public string SiteKey { get; set; } = "";

    public string Secret { get; set; } = "";
}