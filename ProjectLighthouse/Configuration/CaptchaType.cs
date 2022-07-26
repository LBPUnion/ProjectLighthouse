namespace LBPUnion.ProjectLighthouse.Configuration;

/// <summary>
/// The service to be used for presenting captchas to the user.
/// </summary>
public enum CaptchaType
{
    /// <summary>
    /// A privacy-first captcha. https://www.hcaptcha.com/
    /// </summary>
    HCaptcha,
    /// <summary>
    /// A captcha service by Google. https://developers.google.com/recaptcha/
    /// </summary>
    ReCaptcha,
}