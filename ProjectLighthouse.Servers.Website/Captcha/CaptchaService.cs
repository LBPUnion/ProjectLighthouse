using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Captcha;

public class CaptchaService : ICaptchaService
{
    private readonly HttpClient client;

    public CaptchaService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<bool> VerifyCaptcha(HttpRequest request)
    {
        if (!ServerConfiguration.Instance.Captcha.CaptchaEnabled) return true;

        string keyName = ServerConfiguration.Instance.Captcha.Type switch
        {
            CaptchaType.HCaptcha => "h-captcha-response",
            CaptchaType.ReCaptcha => "g-recaptcha-response",
            _ => throw new ArgumentOutOfRangeException(nameof(request),
                @$"Unknown captcha type: {ServerConfiguration.Instance.Captcha.Type}"),
        };

        bool gotCaptcha = request.Form.TryGetValue(keyName, out StringValues values);
        if (!gotCaptcha) return false;

        string? captchaToken = values[0];
        if (captchaToken == null) return false;

        List<KeyValuePair<string, string>> payload = new()
        {
            new KeyValuePair<string, string>("secret", ServerConfiguration.Instance.Captcha.Secret),
            new KeyValuePair<string, string>("response", captchaToken),
        };

        try
        {
            using HttpResponseMessage response =
                await this.client.PostAsync("siteverify", new FormUrlEncodedContent(payload));
            if (!response.IsSuccessStatusCode) return false;

            string responseBody = await response.Content.ReadAsStringAsync();

            // We only really care about the success result, so we just parse that
            return bool.Parse(JObject.Parse(responseBody)["success"]?.ToString() ?? "false");
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToDetailedException(), LogArea.HTTP);
        }
        return false;
    }
}