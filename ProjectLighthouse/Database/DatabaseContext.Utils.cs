using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tickets;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    [GeneratedRegex("^[a-zA-Z0-9_.-]{3,16}$")]
    private static partial Regex UsernameRegex();

    public bool IsUsernameValid(string username) => UsernameRegex().IsMatch(username);

    #nullable enable
    public async Task<UserEntity> CreateUser(string username, string password, string? emailAddress = null)
    {
        if (!password.StartsWith('$')) throw new ArgumentException(nameof(password) + " is not a BCrypt hash");

        // 16 is PSN max, 3 is PSN minimum
        if (!ServerStatics.IsUnitTesting || !username.StartsWith("unitTestUser"))
        {
            if (username.Length is > 16 or < 3) throw new ArgumentException(nameof(username) + " is either too long or too short");

            if (!this.IsUsernameValid(username)) throw new ArgumentException(nameof(username) + " does not match the username regex");
        }

        UserEntity? user = await this.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
        if (user != null) return user;

        user = new UserEntity
        {
            Username = username,
            Password = password,
            Biography = "",
            EmailAddress = emailAddress,
        };
        this.Users.Add(user);

        await this.SaveChangesAsync();

        return user;
    }

    public async Task<int> UserIdFromUsername(string? username)
    {
        if (username == null) return 0;
        return await this.Users.Where(u => u.Username == username).Select(u => u.UserId).FirstOrDefaultAsync();
    }

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

    public async Task RemoveUser(UserEntity? user)
    {
        if (user == null) return;
        if (user.Username.Length == 0) return; // don't delete the placeholder user

        LastContactEntity? lastContact = await this.LastContacts.FirstOrDefaultAsync(l => l.UserId == user.UserId);
        if (lastContact != null) this.LastContacts.Remove(lastContact);

        foreach (ModerationCaseEntity modCase in await this.Cases
                     .Where(c => c.CreatorId == user.UserId || c.DismisserId == user.UserId)
                     .ToListAsync())
        {
            if(modCase.DismisserId == user.UserId)
                modCase.DismisserId = null;
            if(modCase.CreatorId == user.UserId)
                modCase.CreatorId = await SlotHelper.GetPlaceholderUserId(this);
        }

        foreach (SlotEntity slot in this.Slots.Where(s => s.CreatorId == user.UserId)) await this.RemoveSlot(slot, false);

        this.HeartedProfiles.RemoveRange(this.HeartedProfiles.Where(h => h.UserId == user.UserId));
        this.PhotoSubjects.RemoveRange(this.PhotoSubjects.Where(s => s.UserId == user.UserId));
        this.HeartedLevels.RemoveRange(this.HeartedLevels.Where(h => h.UserId == user.UserId));
        this.VisitedLevels.RemoveRange(this.VisitedLevels.Where(v => v.UserId == user.UserId));
        this.RatedReviews.RemoveRange(this.RatedReviews.Where(r => r.UserId == user.UserId));
        this.QueuedLevels.RemoveRange(this.QueuedLevels.Where(q => q.UserId == user.UserId));
        this.RatedLevels.RemoveRange(this.RatedLevels.Where(r => r.UserId == user.UserId));
        this.GameTokens.RemoveRange(this.GameTokens.Where(t => t.UserId == user.UserId));
        this.WebTokens.RemoveRange(this.WebTokens.Where(t => t.UserId == user.UserId));
        this.RatedComments.RemoveRange(this.RatedComments.Where(p => p.UserId == user.UserId));
        this.Comments.RemoveRange(this.Comments.Where(c => c.PosterUserId == user.UserId));
        this.Reviews.RemoveRange(this.Reviews.Where(r => r.ReviewerId == user.UserId));
        this.Photos.RemoveRange(this.Photos.Where(p => p.CreatorId == user.UserId));

        this.Users.Remove(user);

        await this.SaveChangesAsync();
    }

    public async Task HeartUser(int userId, UserEntity heartedUser)
    {
        HeartedProfileEntity? heartedProfile = await this.HeartedProfiles.FirstOrDefaultAsync(q => q.UserId == userId && q.HeartedUserId == heartedUser.UserId);
        if (heartedProfile != null) return;

        this.HeartedProfiles.Add(new HeartedProfileEntity
        {
            HeartedUserId = heartedUser.UserId,
            UserId = userId,
        });

        await this.SaveChangesAsync();
    }

    public async Task UnheartUser(int userId, UserEntity heartedUser)
    {
        HeartedProfileEntity? heartedProfile = await this.HeartedProfiles.FirstOrDefaultAsync(q => q.UserId == userId && q.HeartedUserId == heartedUser.UserId);
        if (heartedProfile != null) this.HeartedProfiles.Remove(heartedProfile);

        await this.SaveChangesAsync();
    }

    public async Task BlockUser(int userId, UserEntity blockedUser)
    {
        if (userId == blockedUser.UserId) return;

        UserEntity? user = await this.Users.FindAsync(userId);

        BlockedProfileEntity blockedProfile = new()
        {
            User = user,
            BlockedUser = blockedUser,
        };

        await this.BlockedProfiles.AddAsync(blockedProfile);

        await this.SaveChangesAsync();
    }

    public async Task UnblockUser(int userId, UserEntity blockedUser)
    {
        if (userId == blockedUser.UserId) return;

        await this.BlockedProfiles.RemoveWhere(bp => bp.BlockedUserId == blockedUser.UserId && bp.UserId == userId);
    }

    public async Task<bool> IsUserBlockedBy(int userId, int targetId)
    {
        if (targetId == userId) return false;

        return await this.BlockedProfiles.Has(bp => bp.BlockedUserId == userId && bp.UserId == targetId);
    }

}