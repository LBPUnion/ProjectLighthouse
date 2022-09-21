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
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Middlewares;

public class RateLimitMiddleware : MiddlewareDBContext
{

    // (userId, requestData)
    private static readonly ConcurrentDictionary<IPAddress, List<LighthouseRequest>> recentRequests = new();

    public RateLimitMiddleware(RequestDelegate next) : base(next)
    { }

    /*
     * What needs to be done:
     * Get the ip address from the request
     * Check if the user has already sent too many requests in which case we return code 429
     * Possible check if the ip is from cloudflare (maybe someone configured their shit wrong)
     * add the request to recentRequests
     * Possibly check the response code of the request that it handles to only add if it was a 200?
     *
     * MAJOR TODOs : add a section to the config where people can add general rate limit rules and custom rules for certain endpoints 
     *             : handle custom config rules here
     *             : while I'm here doing middleware shit, migrate GameServerStartup app.Use to middleware classes
     */

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

        RateLimitOptions? rateLimitOptions = GetRateLimitOverride(ctx.Request.Path);

        if (!IsRateLimitEnabled(rateLimitOptions))
        {
            await this.next(ctx);
            return;
        }

        RemoveExpiredEntries(rateLimitOptions);

        Console.WriteLine(
            $@"[DEBUG]: path={ctx.Request.Path} ipAddress={address}, numRequestsForPath={GetNumRequestsForPath(address, ctx.Request.Path)+1}");

        if (GetNumRequestsForPath(address, ctx.Request.Path) + 1 >= GetMaxNumRequests(rateLimitOptions))
        {
            Console.WriteLine(@$"[DEBUG]: Next request expires in {recentRequests[address][0].Timestamp + GetRequestInterval(rateLimitOptions) * 1000 - TimeHelper.TimestampMillis}");
            ctx.Response.StatusCode = 429;
            return;
        }

        LogRequest(address, ctx.Request.Path);

        // Handle request as normal
        await this.next(ctx);
    }

    private static int GetMaxNumRequests(RateLimitOptions? rateLimitOverride) => rateLimitOverride?.RequestsPerInterval ?? ServerConfiguration.Instance.RateLimitConfiguration.GlobalOptions.RequestsPerInterval;

    public static bool IsRateLimitEnabled(RateLimitOptions? rateLimitOverride) => rateLimitOverride?.Enabled ?? ServerConfiguration.Instance.RateLimitConfiguration.GlobalOptions.Enabled;

    private static long GetRequestInterval(RateLimitOptions? rateLimitOverride) => rateLimitOverride?.RequestsPerInterval ?? ServerConfiguration.Instance.RateLimitConfiguration.GlobalOptions.RequestInterval;

    private static RateLimitOptions? GetRateLimitOverride(PathString path)
    {
        Dictionary<string, RateLimitOptions> overrides = ServerConfiguration.Instance.RateLimitConfiguration.RateLimitOverrides;
        return overrides.Keys.Where(s => new Regex(s.Replace("/", @"\/").Replace("*", ".*")).Match(path).Success).Select(s => overrides[s]).FirstOrDefault();
    }

    private static void LogRequest(IPAddress address, PathString path)
    {
        recentRequests.GetOrAdd(address, new List<LighthouseRequest>()).Add(LighthouseRequest.Create(path));
    }

    private static void RemoveExpiredEntries(RateLimitOptions? rateLimitOverride)
    {
        for (int i = recentRequests.Count - 1; i >= 0; i--)
        {
            IPAddress address = recentRequests.ElementAt(i).Key;
            recentRequests[address].RemoveAll(r => TimeHelper.TimestampMillis > r.Timestamp + GetRequestInterval(GetRateLimitOverride(r.Path)));
            // Remove empty entries
            if (recentRequests[address].Count == 0) recentRequests.TryRemove(address, out _);
        }
    }

    private static int GetNumRequestsForPath(IPAddress address, PathString pathString)
    {
        if (!recentRequests.ContainsKey(address)) return 0;

        List<LighthouseRequest> requests = recentRequests[address];
        return requests.Count(r => r.Path == pathString);
    }

    private class LighthouseRequest
    {
        public PathString Path { get; private init; } = "";
        public long Timestamp { get; private init; }

        public static LighthouseRequest Create(PathString path)
        {
            LighthouseRequest request = new()
            {
                Path = path,
                Timestamp = TimeHelper.TimestampMillis,
            };
            return request;
        }
    }
    
}