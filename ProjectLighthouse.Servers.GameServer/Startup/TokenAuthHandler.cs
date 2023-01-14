using System.Security.Claims;
using System.Text.Encodings.Web;
using LBPUnion.ProjectLighthouse.PlayerData;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Startup;

public class TokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly Database database;
    private const string cookie = "MM_AUTH";

    public TokenAuthHandler
    (
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        UrlEncoder encoder,
        ISystemClock clock,
        Database database
        // I said I don't want any damn vegetables (logs)
    ) : base(options, new NullLoggerFactory(), encoder, clock)
    {
        this.database = database;
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        this.Context.Response.StatusCode = 403;
        return Task.CompletedTask;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!this.Context.Request.Cookies.ContainsKey(cookie)) return AuthenticateResult.Fail("No auth cookie");

        GameToken? gameToken = await this.database.GameTokenFromRequest(this.Request);
        if (gameToken == null) return AuthenticateResult.Fail("No game token");

             if (this.Context.Request.Headers.TryGetValue("User-Agent", out StringValues useragent))
             {
                if (useragent == "MM CHTTPClient $Id: HTTPClient.cpp 36247 2009-11-24 16:17:36Z paul $" || useragent == "MM CHTTPClient LBP2 01.00" || useragent == "MM CHTTPClient LBP2 01.02" || useragent == "MM CHTTPClient LBP2 01.03" || useragent == "MM CHTTPClient LBP2 01.04" || useragent == "MM CHTTPClient LBP2 01.05" || useragent == "MM CHTTPClient LBP2 01.06" || useragent == "MM CHTTPClient LBP2 01.07" || useragent == "MM CHTTPClient LBP2 01.08" || useragent == "MM CHTTPClient LBP2 01.09" || useragent == "MM CHTTPClient LBP2 01.10" || useragent == "MM CHTTPClient LBP2 01.11" || useragent == "MM CHTTPClient LBP2 01.12" || useragent == "MM CHTTPClient LBP2 01.13" || useragent == "MM CHTTPClient LBP2 01.14" || useragent == "MM CHTTPClient LBP2 01.15" || useragent == "MM CHTTPClient LBP2 01.16" || useragent == "MM CHTTPClient LBP2 01.17" || useragent == "MM CHTTPClient LBP2 01.18" || useragent == "MM CHTTPClient LBP2 01.19" || useragent == "MM CHTTPClient LBP2 01.20" || useragent == "MM CHTTPClient LBP2 01.21" || useragent == "MM CHTTPClient LBP2 01.22" || useragent == "MM CHTTPClient LBP2 01.23" || useragent == "MM CHTTPClient LBP2 01.24" || useragent == "MM CHTTPClient LBP2 01.25" || useragent == "MM CHTTPClient LBP2 01.27" || useragent == "MM CHTTPClient LBP2 01.28" || useragent == "MM CHTTPClient LBP2 01.29" || useragent == "MM CHTTPClient LBP2 01.30" || useragent == "MM CHTTPClient LBP2 01.31" || useragent == "MM CHTTPClient LBP2 01.32") {
                    return AuthenticateResult.Fail("Forbidden client");
                }
                else if (useragent == "MM CHTTPClient LBP2 01.26") {
                    if (gameToken.GameVersion == GameVersion.LittleBigPlanet2) {
                        return AuthenticateResult.Fail("Forbidden client");
                    }
                }
            }
        this.Context.Items["Token"] = gameToken;
        Claim[] claims = {
            new("userId", gameToken.UserId.ToString()),
        };
        ClaimsIdentity identity = new(claims, this.Scheme.Name);
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, this.Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}