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