#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class RequestExtensions
{
    // yoinked and adapted from https://stackoverflow.com/a/68641796

    #region Mobile Checking

    private static readonly Regex mobileCheck = new
    (
        "Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|PlayStation Vita",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled
    );

    public static bool IsMobile(this HttpRequest request) => mobileCheck.IsMatch(request.Headers[HeaderNames.UserAgent].ToString());

    #endregion

    #region Captcha

    private static readonly HttpClient client = new()
    {
        BaseAddress = new Uri("https://hcaptcha.com"),
    };

    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
    private static async Task<bool> verifyCaptcha(string token)
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

    public static async Task<bool> CheckCaptchaValidity(this HttpRequest request)
    {
        if (ServerConfiguration.Instance.Captcha.CaptchaEnabled)
        {
            bool gotCaptcha = request.Form.TryGetValue("h-captcha-response", out StringValues values);
            if (!gotCaptcha) return false;

            if (!await verifyCaptcha(values[0])) return false;
        }

        return true;
    }

    #endregion

}