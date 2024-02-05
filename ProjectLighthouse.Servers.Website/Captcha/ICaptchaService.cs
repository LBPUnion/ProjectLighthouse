namespace LBPUnion.ProjectLighthouse.Servers.Website.Captcha;

public interface ICaptchaService
{
    Task<bool> VerifyCaptcha(HttpRequest request);
}