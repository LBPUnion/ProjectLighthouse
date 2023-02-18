#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Middlewares;

public class RateLimitMiddleware : Middleware
{

    // (ipAddress, requestData)
    private static readonly ConcurrentDictionary<IPAddress, ConcurrentQueue<LighthouseRequest?>> recentRequests = new();

    public RateLimitMiddleware(RequestDelegate next) : base(next)
    { }

    public override async Task InvokeAsync(HttpContext ctx)
    {
        // We only want to rate limit POST requests
        if (ctx.Request.Method != "POST")
        {
            await this.next(ctx);
            return;
        }
        IPAddress? address = ctx.Connection.RemoteIpAddress;
        if (address == null)
        {
            await this.next(ctx);
            return;
        }

        PathString path = RemoveTrailingSlash(ctx.Request.Path.ToString());

        RateLimitOptions? options = GetRateLimitOverride(path);

        if (!IsRateLimitEnabled(options))
        {
            await this.next(ctx);
            return;
        }

        RemoveExpiredEntries();

        if (GetNumRequestsForPath(address, path, options) >= GetMaxNumRequests(options))
        {
            Logger.Info($"Request limit reached for {address} ({ctx.Request.Path})", LogArea.RateLimit);
            recentRequests[address].TryPeek(out LighthouseRequest? request);
            long nextExpiration = request?.Expiration ?? TimeHelper.TimestampMillis;
            ctx.Response.Headers.TryAdd("Retry-After", "" + Math.Ceiling((nextExpiration - TimeHelper.TimestampMillis) / 1000f));
            ctx.Response.StatusCode = 429;
            await ctx.Response.WriteAsync(
                "<html><head><title>Rate limit reached</title><style>html{font-family: Tahoma, Verdana, Arial, sans-serif;}</style></head>" +
                "<h1>You have reached the rate limit</h1>" +
                $"<p>Try again in {ctx.Response.Headers.RetryAfter} seconds</html>");
            return;
        }

        LogRequest(address, path, options);

        // Handle request as normal
        await this.next(ctx);
    }

    private static int GetMaxNumRequests(RateLimitOptions? options) => options?.RequestsPerInterval ?? ServerConfiguration.Instance.RateLimitConfiguration.GlobalOptions.RequestsPerInterval;

    private static bool IsRateLimitEnabled(RateLimitOptions? options) => options?.Enabled ?? ServerConfiguration.Instance.RateLimitConfiguration.GlobalOptions.Enabled;

    private static long GetRequestInterval(RateLimitOptions? options) => options?.RequestInterval ?? ServerConfiguration.Instance.RateLimitConfiguration.GlobalOptions.RequestInterval;

    private static RateLimitOptions? GetRateLimitOverride(PathString path)
    {
        Dictionary<string, RateLimitOptions> overrides = ServerConfiguration.Instance.RateLimitConfiguration.OverrideOptions;
        List<string> matchingOptions = overrides.Keys.Where(s => new Regex("^" + s.Replace("/", @"\/").Replace("*", ".*") + "$").Match(path).Success).ToList();
        if (matchingOptions.Count == 0) return null;
        // return 0 for equal, -1 for a, and 1 for b
        matchingOptions.Sort((a, b) =>
        {
            int aWeight = 100;
            int bWeight = 100;
            if (a.Contains('*')) aWeight -= 20;
            if (b.Contains('*')) bWeight -= 20;

            aWeight += a.Length;
            bWeight += b.Length;

            if (aWeight > bWeight) return -1;

            if (bWeight > aWeight) return 1;

            return 0;
        });
        return overrides[matchingOptions.First()];
    }

    private static void LogRequest(IPAddress address, PathString path, RateLimitOptions? options)
    {
        LighthouseRequest request = LighthouseRequest.Create(path, GetRequestInterval(options) * 1000 + TimeHelper.TimestampMillis, options);
        recentRequests.GetOrAdd(address, new ConcurrentQueue<LighthouseRequest?>()).Enqueue(request);
    }

    private static void RemoveExpiredEntries()
    {
        foreach((IPAddress address, ConcurrentQueue<LighthouseRequest?> list) in recentRequests)
        {
            if (list.IsEmpty)
            {
                recentRequests.TryRemove(address, out _);
                continue;
            }

            while (list.TryPeek(out LighthouseRequest? request))
            {
                if (TimeHelper.TimestampMillis < (request?.Expiration ?? TimeHelper.TimestampMillis)) 
                    break;

                list.TryDequeue(out _);
            }
        }
    }

    private static string RemoveTrailingSlash(string s) => s.TrimEnd('/').TrimEnd('\\');

    private static int GetNumRequestsForPath(IPAddress address, PathString path, RateLimitOptions? options)
    {
        if (!recentRequests.ContainsKey(address)) return 0;
        int? optionsHash = options?.GetHashCode();
        // If there are no custom options then count requests based on exact url matches, otherwise use regex matching
        return options switch
        {
            null => recentRequests[address].Count(r => (r?.Path ?? "") == path),
            _ => recentRequests[address].Count(r => r?.OptionsHash == optionsHash),
        };
    }

    private class LighthouseRequest
    {
        public PathString Path { get; private init; } = "";
        public int? OptionsHash { get; private init; }
        public long Expiration { get; private init; }

        public static LighthouseRequest Create(PathString path, long expiration, RateLimitOptions? options = null)
        {
            LighthouseRequest request = new()
            {
                Path = path,
                Expiration = expiration,
                OptionsHash = options?.GetHashCode(),
            };
            return request;
        }
    }
    
}