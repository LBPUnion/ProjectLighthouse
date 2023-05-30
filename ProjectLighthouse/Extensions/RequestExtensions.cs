#nullable enable
using System.Text.RegularExpressions;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Filter;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static partial class RequestExtensions
{

    public static PaginationData GetPaginationData(this HttpRequest request)
    {
        int start = int.TryParse(request.Query["pageStart"], out int pageStart) ? pageStart : 0;
        int size = int.TryParse(request.Query["pageSize"], out int pageSize)
            ? pageSize
            : ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots;

        if (start < 0) start = 0;
        if (size <= 0) size = 10;

        PaginationData paginationData = new()
        {
            PageStart = start,
            PageSize = size,
        };
        return paginationData;
    }

    #region Mobile Checking

    // yoinked and adapted from https://stackoverflow.com/a/68641796
    [GeneratedRegex("Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|PlayStation Vita",
        RegexOptions.IgnoreCase | RegexOptions.Multiline,
        "en-US")]
    private static partial Regex MobileCheckRegex();

    public static bool IsMobile(this HttpRequest request) => MobileCheckRegex().IsMatch(request.Headers[HeaderNames.UserAgent].ToString());

    #endregion
}