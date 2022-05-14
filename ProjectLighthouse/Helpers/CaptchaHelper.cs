using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Newtonsoft.Json.Linq;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class CaptchaHelper
{
    private static readonly HttpClient client = new()
    {
        BaseAddress = new Uri("https://hcaptcha.com"),
    };

    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
    public static async Task<bool> Verify(string token)
    {
        if (!ServerConfiguration.Instance.Captcha.CaptchaEnabled) return true;

        List<KeyValuePair<string, string>> payload = new()
        {
            new("secret", ServerConfiguration.Instance.Captcha.Secret),
            new("response", token),
        };

        HttpResponseMessage response = await client.PostAsync("/siteverify", new FormUrlEncodedContent(payload));

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();

        // We only really care about the success result, nothing else that hcaptcha sends us, so lets only parse that.
        bool success = bool.Parse(JObject.Parse(responseBody)["success"]?.ToString() ?? "false");
        return success;
    }
}