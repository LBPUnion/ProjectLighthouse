#nullable enable
using System;
using System.Collections.Generic;
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
    public async Task<string> UsernameFromWebToken(WebTokenEntity? token)
    {
        if (token == null) return "";

        return await this.Users.Where(u => u.UserId == token.UserId).Select(u => u.Username).FirstAsync();
    }

    private UserEntity? UserFromLighthouseToken(string lighthouseToken)
    {
        WebTokenEntity? token = this.WebTokens.FirstOrDefault(t => t.UserToken == lighthouseToken);
        if (token == null) return null;

        if (DateTime.UtcNow <= token.ExpiresAt) return this.Users.FirstOrDefault(u => u.UserId == token.UserId);

        this.Remove(token);
        this.SaveChanges();

        return null;
    }

    public UserEntity? UserFromWebRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("LighthouseToken", out string? lighthouseToken)) return null;

        return this.UserFromLighthouseToken(lighthouseToken);
    }

    public WebTokenEntity? WebTokenFromRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("LighthouseToken", out string? lighthouseToken)) return null;

        WebTokenEntity? token = this.WebTokens.FirstOrDefault(t => t.UserToken == lighthouseToken);
        if (token == null) return null;

        if (DateTime.UtcNow <= token.ExpiresAt) return token;

        this.Remove(token);
        this.SaveChanges();

        return null;

    }

    public async Task<UserEntity?> UserFromPasswordResetToken(string? resetToken)
    {
        if (string.IsNullOrWhiteSpace(resetToken)) return null;

        PasswordResetTokenEntity? token =
            await this.PasswordResetTokens.FirstOrDefaultAsync(token => token.ResetToken == resetToken);
        if (token == null) return null;

        if (token.Created >= DateTime.UtcNow.AddHours(-1))
            return await this.Users.FirstOrDefaultAsync(user => user.UserId == token.UserId);

        this.PasswordResetTokens.Remove(token);
        await this.SaveChangesAsync();

        return null;
    }

    public bool IsRegistrationTokenValid(string? tokenString)
    {
        if (string.IsNullOrWhiteSpace(tokenString)) return false;

        RegistrationTokenEntity? token = this.RegistrationTokens.FirstOrDefault(t => t.Token == tokenString);
        if (token == null) return false;

        if (token.Created >= DateTime.UtcNow.AddDays(-7)) return true;

        this.RegistrationTokens.Remove(token);
        this.SaveChanges();

        return false;

    }

    public async Task RemoveExpiredTokens()
    {
        List<GameTokenEntity> expiredTokens = await this.GameTokens.Where(t => DateTime.UtcNow > t.ExpiresAt).ToListAsync(); 
        foreach (GameTokenEntity token in expiredTokens)
        {
            UserEntity? user = await this.Users.FirstOrDefaultAsync(u => u.UserId == token.UserId);
            if (user != null) user.LastLogout = TimeHelper.TimestampMillis;
        }
        await this.SaveChangesAsync();

        await this.GameTokens.RemoveWhere(t => DateTime.UtcNow > t.ExpiresAt);
        await this.WebTokens.RemoveWhere(t => DateTime.UtcNow > t.ExpiresAt);
        await this.EmailVerificationTokens.RemoveWhere(t => DateTime.UtcNow > t.ExpiresAt);
        await this.EmailSetTokens.RemoveWhere(t => DateTime.UtcNow > t.ExpiresAt);
        await this.PasswordResetTokens.RemoveWhere(t => DateTime.UtcNow > t.Created.AddDays(1));
    }

    public async Task RemoveRegistrationToken(string? tokenString)
    {
        if (string.IsNullOrWhiteSpace(tokenString)) return;

        RegistrationTokenEntity? token = await this.RegistrationTokens.FirstOrDefaultAsync(t => t.Token == tokenString);
        if (token == null) return;

        this.RegistrationTokens.Remove(token);

        await this.SaveChangesAsync();
    }
}