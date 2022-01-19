using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace LBPUnion.ProjectLighthouse.Helpers.Extensions;

// yoinked and adapted from https://stackoverflow.com/a/68641796
public static class RequestExtensions
{
    private static readonly Regex mobileCheck = new
        ("Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

    public static bool IsMobile(this HttpRequest request) => mobileCheck.IsMatch(request.Headers[HeaderNames.UserAgent].ToString());
}