#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{

    public async Task<string> UsernameFromGameToken(GameToken? token)
    {
        if (token == null) return "";

        return await this.Users.Where(u => u.UserId == token.UserId).Select(u => u.Username).FirstAsync();
    }

    public async Task<User?> UserFromGameToken(GameToken? token)
    {
        if (token == null) return null;

        return await this.Users.FirstOrDefaultAsync(u => u.UserId == token.UserId);
    }

    private async Task<User?> UserFromMMAuth(string authToken)
    {
        GameToken? token = await this.GameTokens.FirstOrDefaultAsync(t => t.UserToken == authToken);

        if (token == null) return null;

        if (DateTime.Now <= token.ExpiresAt) return await this.Users.FirstOrDefaultAsync(u => u.UserId == token.UserId);

        this.Remove(token);
        await this.SaveChangesAsync();

        return null;
    }

    public async Task<User?> UserFromGameRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth)) return null;

        return await this.UserFromMMAuth(mmAuth);
    }

    public async Task<GameToken?> GameTokenFromRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth)) return null;

        GameToken? token = await this.GameTokens.FirstOrDefaultAsync(t => t.UserToken == mmAuth);

        if (token == null) return null;

        if (DateTime.Now <= token.ExpiresAt) return token;

        this.Remove(token);
        await this.SaveChangesAsync();

        return null;
    }

}