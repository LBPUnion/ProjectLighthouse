#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class RequestExtensions
{
    static RequestExtensions()
    {
        Uri captchaUri = ServerConfiguration.Instance.Captcha.Type switch
        {
            CaptchaType.HCaptcha => new Uri("https://hcaptcha.com"),
            CaptchaType.ReCaptcha => new Uri("https://www.google.com/recaptcha/api/"),
            _ => throw new ArgumentOutOfRangeException(),
        };
        
        client = new HttpClient
        {
            BaseAddress = captchaUri,
        };
    }

    #region URL stuff

    public static string GetBaseUrl(this HttpRequest request)
    {
        // this sucks
        string listenUrl = ServerStatics.ServerType switch
        {
            ServerType.GameServer => ServerConfiguration.Instance.GameApiListenUrl,
            ServerType.Website => ServerConfiguration.Instance.WebsiteListenUrl,
            ServerType.Api => ServerConfiguration.Instance.ApiListenUrl,
            _ => ServerConfiguration.Instance.ExternalUrl,
        };

        string displayUrl = request.GetDisplayUrl();
        if (!new Uri(displayUrl).IsDefaultPort) return displayUrl[..displayUrl.IndexOf(request.Path)];

        displayUrl = displayUrl[..displayUrl.IndexOf(request.Path)] + ":" + listenUrl[(listenUrl.LastIndexOf(":", StringComparison.Ordinal)+1)..];
        return displayUrl;
    }

    #endregion
    
    #region Mobile Checking

    // yoinked and adapted from https://stackoverflow.com/a/68641796

    private static readonly Regex mobileCheck = new
    (
        "Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|PlayStation Vita",
        RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled
    );

    public static bool IsMobile(this HttpRequest request) => mobileCheck.IsMatch(request.Headers[HeaderNames.UserAgent].ToString());

    #endregion

    #region Captcha

    private static readonly HttpClient client;

    [SuppressMessage("ReSharper", "ArrangeObjectCreationWhenTypeNotEvident")]
    private static async Task<bool> verifyCaptcha(string token)
    {
        if (!ServerConfiguration.Instance.Captcha.CaptchaEnabled) return true;

        List<KeyValuePair<string, string>> payload = new()
        {
            new("secret", ServerConfiguration.Instance.Captcha.Secret),
            new("response", token),
        };

        HttpResponseMessage response = await client.PostAsync("siteverify", new FormUrlEncodedContent(payload));

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
            string keyName = ServerConfiguration.Instance.Captcha.Type switch
            {
                CaptchaType.HCaptcha => "h-captcha-response",
                CaptchaType.ReCaptcha => "g-recaptcha-response",
                _ => throw new ArgumentOutOfRangeException(),
            };
            
            bool gotCaptcha = request.Form.TryGetValue(keyName, out StringValues values);
            if (!gotCaptcha) return false;

            if (!await verifyCaptcha(values[0])) return false;
        }

        return true;
    }

    #endregion

}