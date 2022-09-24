#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Middlewares;

public class RateLimitMiddleware : MiddlewareDBContext
{

    // (userId, requestData)
    private static readonly ConcurrentDictionary<IPAddress, List<LighthouseRequest>> recentRequests = new();

    public RateLimitMiddleware(RequestDelegate next) : base(next)
    { }

    public override async Task InvokeAsync(HttpContext ctx, Database database)
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

        if (GetNumRequestsForPath(address, path) >= GetMaxNumRequests(options))
        {
            Logger.Info($"Request limit reached for {address.ToString()} ({ctx.Request.Path})", LogArea.RateLimit);
            ctx.Response.Headers.Add("Retry-After", "" + Math.Ceiling((recentRequests[address][0].Expiration - TimeHelper.TimestampMillis) / 1000f));
            ctx.Response.StatusCode = 429;
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
        recentRequests.GetOrAdd(address, new List<LighthouseRequest>()).Add(LighthouseRequest.Create(path, GetRequestInterval(options) * 1000 + TimeHelper.TimestampMillis));
    }

    private static void RemoveExpiredEntries()
    {
        for (int i = recentRequests.Count - 1; i >= 0; i--)
        {
            IPAddress address = recentRequests.ElementAt(i).Key;
            recentRequests[address].RemoveAll(r => TimeHelper.TimestampMillis > r.Expiration);
            // Remove empty entries
            if (recentRequests[address].Count == 0) recentRequests.TryRemove(address, out _);
        }
    }

    private static string RemoveTrailingSlash(string s) => s.TrimEnd('/').TrimEnd('\\');

    private static int GetNumRequestsForPath(IPAddress address, PathString path)
    {
        if (!recentRequests.ContainsKey(address)) return 0;

        List<LighthouseRequest> requests = recentRequests[address];
        return requests.Count(r => r.Path == path);
    }

    private class LighthouseRequest
    {
        public PathString Path { get; private init; } = "";
        public long Expiration { get; private init; }

        public static LighthouseRequest Create(PathString path, long expiration)
        {
            LighthouseRequest request = new()
            {
                Path = path,
                Expiration = expiration,
            };
            return request;
        }
    }
    
}