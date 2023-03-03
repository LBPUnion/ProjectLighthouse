#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    public async Task<string> UsernameFromWebToken(WebToken? token)
    {
        if (token == null) return "";

        return await this.Users.Where(u => u.UserId == token.UserId).Select(u => u.Username).FirstAsync();
    }

    private User? UserFromLighthouseToken(string lighthouseToken)
    {
        WebToken? token = this.WebTokens.FirstOrDefault(t => t.UserToken == lighthouseToken);
        if (token == null) return null;

        if (DateTime.Now <= token.ExpiresAt) return this.Users.FirstOrDefault(u => u.UserId == token.UserId);

        this.Remove(token);
        this.SaveChanges();

        return null;
    }

    public User? UserFromWebRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("LighthouseToken", out string? lighthouseToken)) return null;

        return this.UserFromLighthouseToken(lighthouseToken);
    }

    public WebToken? WebTokenFromRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("LighthouseToken", out string? lighthouseToken)) return null;

        WebToken? token = this.WebTokens.FirstOrDefault(t => t.UserToken == lighthouseToken);
        if (token == null) return null;

        if (DateTime.Now <= token.ExpiresAt) return token;

        this.Remove(token);
        this.SaveChanges();

        return null;

    }

    public async Task<User?> UserFromPasswordResetToken(string? resetToken)
    {
        if (string.IsNullOrWhiteSpace(resetToken)) return null;

        PasswordResetToken? token =
            await this.PasswordResetTokens.FirstOrDefaultAsync(token => token.ResetToken == resetToken);
        if (token == null) return null;

        if (token.Created >= DateTime.Now.AddHours(-1))
            return await this.Users.FirstOrDefaultAsync(user => user.UserId == token.UserId);

        this.PasswordResetTokens.Remove(token);
        await this.SaveChangesAsync();

        return null;
    }

    public bool IsRegistrationTokenValid(string? tokenString)
    {
        if (string.IsNullOrWhiteSpace(tokenString)) return false;

        RegistrationToken? token = this.RegistrationTokens.FirstOrDefault(t => t.Token == tokenString);
        if (token == null) return false;

        if (token.Created >= DateTime.Now.AddDays(-7)) return true;

        this.RegistrationTokens.Remove(token);
        this.SaveChanges();

        return false;

    }

    public async Task RemoveExpiredTokens()
    {
        foreach (GameToken token in await this.GameTokens.Where(t => DateTime.Now > t.ExpiresAt).ToListAsync())
        {
            User? user = await this.Users.FirstOrDefaultAsync(u => u.UserId == token.UserId);
            if (user != null) user.LastLogout = TimeHelper.TimestampMillis;
            this.GameTokens.Remove(token);
        }

        this.WebTokens.RemoveWhere(t => DateTime.Now > t.ExpiresAt);
        this.EmailVerificationTokens.RemoveWhere(t => DateTime.Now > t.ExpiresAt);
        this.EmailSetTokens.RemoveWhere(t => DateTime.Now > t.ExpiresAt);

        await this.SaveChangesAsync();
    }

    public async Task RemoveRegistrationToken(string? tokenString)
    {
        if (string.IsNullOrWhiteSpace(tokenString)) return;

        RegistrationToken? token = await this.RegistrationTokens.FirstOrDefaultAsync(t => t.Token == tokenString);
        if (token == null) return;

        this.RegistrationTokens.Remove(token);

        await this.SaveChangesAsync();
    }
}