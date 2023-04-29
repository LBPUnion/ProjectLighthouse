#nullable enable
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static partial class RequestExtensions
{
    #region Mobile Checking

    // yoinked and adapted from https://stackoverflow.com/a/68641796
    [GeneratedRegex("Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|PlayStation Vita",
        RegexOptions.IgnoreCase | RegexOptions.Multiline,
        "en-US")]
    private static partial Regex MobileCheckRegex();

    public static bool IsMobile(this HttpRequest request) => MobileCheckRegex().IsMatch(request.Headers[HeaderNames.UserAgent].ToString());

    #endregion
}