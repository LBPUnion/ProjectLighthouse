#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tickets;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    public async Task<GameTokenEntity?> AuthenticateUser(UserEntity? user, NPTicket npTicket, string userLocation)
    {
        if (user == null) return null;

        GameTokenEntity gameToken = new()
        {
            UserToken = CryptoHelper.GenerateAuthToken(),
            User = user,
            UserId = user.UserId,
            UserLocation = userLocation,
            GameVersion = npTicket.GameVersion,
            Platform = npTicket.Platform,
            TicketHash = npTicket.TicketHash,
            // we can get away with a low expiry here since LBP will just get a new token everytime it gets 403'd
            ExpiresAt = DateTime.Now + TimeSpan.FromHours(1),
        };

        this.GameTokens.Add(gameToken);
        await this.SaveChangesAsync();

        return gameToken;
    }

    public async Task<string> UsernameFromGameToken(GameTokenEntity? token)
    {
        if (token == null) return "";

        return await this.Users.Where(u => u.UserId == token.UserId).Select(u => u.Username).FirstAsync();
    }

    public async Task<UserEntity?> UserFromGameToken(GameTokenEntity? token)
    {
        if (token == null) return null;

        return await this.Users.FindAsync(token.UserId);
    }

    public async Task<GameTokenEntity?> GameTokenFromRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth)) return null;

        GameTokenEntity? token = await this.GameTokens.FirstOrDefaultAsync(t => t.UserToken == mmAuth);

        if (token == null) return null;

        if (DateTime.Now <= token.ExpiresAt) return token;

        this.Remove(token);
        await this.SaveChangesAsync();

        return null;
    }

}