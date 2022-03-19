using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Categories;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using LBPUnion.ProjectLighthouse.Types.Profiles.Email;
using LBPUnion.ProjectLighthouse.Types.Reports;
using LBPUnion.ProjectLighthouse.Types.Reviews;
using LBPUnion.ProjectLighthouse.Types.Settings;
using LBPUnion.ProjectLighthouse.Types.Tickets;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse;

public class Database : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Slot> Slots { get; set; }
    public DbSet<QueuedLevel> QueuedLevels { get; set; }
    public DbSet<HeartedLevel> HeartedLevels { get; set; }
    public DbSet<HeartedProfile> HeartedProfiles { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<GameToken> GameTokens { get; set; }
    public DbSet<WebToken> WebTokens { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<PhotoSubject> PhotoSubjects { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<LastContact> LastContacts { get; set; }
    public DbSet<VisitedLevel> VisitedLevels { get; set; }
    public DbSet<RatedLevel> RatedLevels { get; set; }
    public DbSet<AuthenticationAttempt> AuthenticationAttempts { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<RatedReview> RatedReviews { get; set; }
    public DbSet<UserApprovedIpAddress> UserApprovedIpAddresses { get; set; }
    public DbSet<DatabaseCategory> CustomCategories { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<GriefReport> Reports { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<EmailSetToken> EmailSetTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseMySql(ServerSettings.Instance.DbConnectionString, MySqlServerVersion.LatestSupportedServerVersion);

    #nullable enable
    public async Task<User> CreateUser(string username, string password, string? emailAddress = null)
    {
        if (!password.StartsWith('$')) throw new ArgumentException(nameof(password) + " is not a BCrypt hash");

        // 16 is PSN max, 3 is PSN minimum
        if (!ServerStatics.IsUnitTesting || !username.StartsWith("unitTestUser"))
        {
            if (username.Length > 16 || username.Length < 3) throw new ArgumentException(nameof(username) + " is either too long or too short");

            Regex regex = new("^[a-zA-Z0-9_.-]*$");

            if (!regex.IsMatch(username)) throw new ArgumentException(nameof(username) + " does not match the username regex");
        }

        User? user = await this.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
        if (user != null) return user;

        Location l = new(); // store to get id after submitting
        this.Locations.Add(l); // add to table
        await this.SaveChangesAsync(); // saving to the database returns the id and sets it on this entity

        user = new User
        {
            Username = username,
            Password = password,
            LocationId = l.Id,
            Biography = "",
            EmailAddress = emailAddress,
        };
        this.Users.Add(user);

        await this.SaveChangesAsync();

        if (emailAddress != null && ServerSettings.Instance.SMTPEnabled)
        {
            string body = "An account for Project Lighthouse has been registered with this email address.\n\n" +
                          $"You can login at {ServerSettings.Instance.ExternalUrl}.";

            SMTPHelper.SendEmail(emailAddress, "Project Lighthouse Account Created: " + username, body);
        }

        return user;
    }

    public async Task<GameToken?> AuthenticateUser(NPTicket npTicket, string userLocation)
    {
        User? user = await this.Users.FirstOrDefaultAsync(u => u.Username == npTicket.Username);
        if (user == null) return null;

        GameToken gameToken = new()
        {
            UserToken = HashHelper.GenerateAuthToken(),
            User = user,
            UserId = user.UserId,
            UserLocation = userLocation,
            GameVersion = npTicket.GameVersion,
            Platform = npTicket.Platform,
        };

        this.GameTokens.Add(gameToken);
        await this.SaveChangesAsync();

        return gameToken;
    }

    #region Hearts & Queues

    public async Task<bool> RateComment(User user, int commentId, int rating)
    {
        Comment? comment = await this.Comments.FirstOrDefaultAsync(c => commentId == c.CommentId);

        if (comment == null) return false;

        if (comment.PosterUserId == user.UserId) return false;

        Reaction? reaction = await this.Reactions.FirstOrDefaultAsync(r => r.UserId == user.UserId && r.TargetId == commentId);
        if (reaction == null)
        {
            Reaction newReaction = new()
            {
                UserId = user.UserId,
                TargetId = commentId,
                Rating = 0,
            };
            this.Reactions.Add(newReaction);
            await this.SaveChangesAsync();
            reaction = newReaction;
        }

        int oldRating = reaction.Rating;
        if (oldRating == rating) return true;

        reaction.Rating = rating;
        // if rating changed then we count the number of reactions to ensure accuracy
        List<Reaction> reactions = await this.Reactions.Where(c => c.TargetId == commentId).ToListAsync();
        int yay = 0;
        int boo = 0;
        foreach (Reaction r in reactions)
        {
            switch (r.Rating)
            {
                case -1:
                    boo++;
                    break;
                case 1:
                    yay++;
                    break;
            }
        }

        comment.ThumbsDown = boo;
        comment.ThumbsUp = yay;
        await this.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PostComment(User user, int targetId, CommentType type, string message)
    {
        if (message.Length > 100) return false;

        if (type == CommentType.Profile)
        {
            User? targetUser = await this.Users.FirstOrDefaultAsync(u => u.UserId == targetId);
            if (targetUser == null) return false;
        }
        else
        {
            Slot? targetSlot = await this.Slots.FirstOrDefaultAsync(u => u.SlotId == targetId);
            if (targetSlot == null) return false;
        }

        this.Comments.Add
        (
            new Comment
            {
                PosterUserId = user.UserId,
                TargetId = targetId,
                Type = type,
                Message = message,
                Timestamp = TimeHelper.UnixTimeMilliseconds(),
            }
        );
        await this.SaveChangesAsync();
        return true;
    }

    public async Task HeartUser(User user, User heartedUser)
    {
        HeartedProfile? heartedProfile = await this.HeartedProfiles.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.HeartedUserId == heartedUser.UserId);
        if (heartedProfile != null) return;

        this.HeartedProfiles.Add
        (
            new HeartedProfile
            {
                HeartedUserId = heartedUser.UserId,
                UserId = user.UserId,
            }
        );

        await this.SaveChangesAsync();
    }

    public async Task UnheartUser(User user, User heartedUser)
    {
        HeartedProfile? heartedProfile = await this.HeartedProfiles.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.HeartedUserId == heartedUser.UserId);
        if (heartedProfile != null) this.HeartedProfiles.Remove(heartedProfile);

        await this.SaveChangesAsync();
    }

    public async Task HeartLevel(User user, Slot heartedSlot)
    {
        HeartedLevel? heartedLevel = await this.HeartedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == heartedSlot.SlotId);
        if (heartedLevel != null) return;

        this.HeartedLevels.Add
        (
            new HeartedLevel
            {
                SlotId = heartedSlot.SlotId,
                UserId = user.UserId,
            }
        );

        await this.SaveChangesAsync();
    }

    public async Task UnheartLevel(User user, Slot heartedSlot)
    {
        HeartedLevel? heartedLevel = await this.HeartedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == heartedSlot.SlotId);
        if (heartedLevel != null) this.HeartedLevels.Remove(heartedLevel);

        await this.SaveChangesAsync();
    }

    public async Task QueueLevel(User user, Slot queuedSlot)
    {
        QueuedLevel? queuedLevel = await this.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == queuedSlot.SlotId);
        if (queuedLevel != null) return;

        this.QueuedLevels.Add
        (
            new QueuedLevel
            {
                SlotId = queuedSlot.SlotId,
                UserId = user.UserId,
            }
        );

        await this.SaveChangesAsync();
    }

    public async Task UnqueueLevel(User user, Slot queuedSlot)
    {
        QueuedLevel? queuedLevel = await this.QueuedLevels.FirstOrDefaultAsync(q => q.UserId == user.UserId && q.SlotId == queuedSlot.SlotId);
        if (queuedLevel != null) this.QueuedLevels.Remove(queuedLevel);

        await this.SaveChangesAsync();
    }

    #endregion

    #region Game Token Shenanigans

    public async Task<User?> UserFromMMAuth(string authToken, bool allowUnapproved = false)
    {
        if (ServerStatics.IsUnitTesting) allowUnapproved = true;
        GameToken? token = await this.GameTokens.FirstOrDefaultAsync(t => t.UserToken == authToken);

        if (token == null) return null;
        if (!allowUnapproved && !token.Approved) return null;

        return await this.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == token.UserId);
    }

    public async Task<User?> UserFromGameToken
        (GameToken gameToken, bool allowUnapproved = false)
        => await this.UserFromMMAuth(gameToken.UserToken, allowUnapproved);

    public async Task<User?> UserFromGameRequest(HttpRequest request, bool allowUnapproved = false)
    {
        if (ServerStatics.IsUnitTesting) allowUnapproved = true;
        if (!request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth) || mmAuth == null) return null;

        return await this.UserFromMMAuth(mmAuth, allowUnapproved);
    }

    public async Task<GameToken?> GameTokenFromRequest(HttpRequest request, bool allowUnapproved = false)
    {
        if (ServerStatics.IsUnitTesting) allowUnapproved = true;
        if (!request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth) || mmAuth == null) return null;

        GameToken? token = await this.GameTokens.FirstOrDefaultAsync(t => t.UserToken == mmAuth);

        if (token == null) return null;
        if (!allowUnapproved && !token.Approved) return null;

        return token;
    }

    public async Task<(User, GameToken)?> UserAndGameTokenFromRequest(HttpRequest request, bool allowUnapproved = false)
    {
        if (ServerStatics.IsUnitTesting) allowUnapproved = true;
        if (!request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth) || mmAuth == null) return null;

        GameToken? token = await this.GameTokens.FirstOrDefaultAsync(t => t.UserToken == mmAuth);
        if (token == null) return null;
        if (!allowUnapproved && !token.Approved) return null;

        User? user = await this.UserFromGameToken(token);

        if (user == null) return null;

        return (user, token);
    }

    #endregion

    #region Web Token Shenanigans

    public User? UserFromLighthouseToken(string lighthouseToken)
    {
        WebToken? token = this.WebTokens.FirstOrDefault(t => t.UserToken == lighthouseToken);
        if (token == null) return null;

        return this.Users.Include(u => u.Location).FirstOrDefault(u => u.UserId == token.UserId);
    }

    public User? UserFromWebRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("LighthouseToken", out string? lighthouseToken) || lighthouseToken == null) return null;

        return this.UserFromLighthouseToken(lighthouseToken);
    }

    public WebToken? WebTokenFromRequest(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue("LighthouseToken", out string? lighthouseToken) || lighthouseToken == null) return null;

        return this.WebTokens.FirstOrDefault(t => t.UserToken == lighthouseToken);
    }

    #endregion

    public async Task<Photo?> PhotoFromSubject(PhotoSubject subject)
        => await this.Photos.FirstOrDefaultAsync(p => p.PhotoSubjectIds.Contains(subject.PhotoSubjectId.ToString()));
    public async Task RemoveUser(User? user)
    {
        if (user == null) return;

        if (user.Location != null) this.Locations.Remove(user.Location);
        LastContact? lastContact = await this.LastContacts.FirstOrDefaultAsync(l => l.UserId == user.UserId);
        if (lastContact != null) this.LastContacts.Remove(lastContact);

        foreach (Slot slot in this.Slots.Where(s => s.CreatorId == user.UserId)) await this.RemoveSlot(slot, false);

        this.AuthenticationAttempts.RemoveRange(this.AuthenticationAttempts.Include(a => a.GameToken).Where(a => a.GameToken.UserId == user.UserId));
        this.HeartedProfiles.RemoveRange(this.HeartedProfiles.Where(h => h.UserId == user.UserId));
        this.PhotoSubjects.RemoveRange(this.PhotoSubjects.Where(s => s.UserId == user.UserId));
        this.HeartedLevels.RemoveRange(this.HeartedLevels.Where(h => h.UserId == user.UserId));
        this.VisitedLevels.RemoveRange(this.VisitedLevels.Where(v => v.UserId == user.UserId));
        this.RatedReviews.RemoveRange(this.RatedReviews.Where(r => r.UserId == user.UserId));
        this.QueuedLevels.RemoveRange(this.QueuedLevels.Where(q => q.UserId == user.UserId));
        this.RatedLevels.RemoveRange(this.RatedLevels.Where(r => r.UserId == user.UserId));
        this.GameTokens.RemoveRange(this.GameTokens.Where(t => t.UserId == user.UserId));
        this.WebTokens.RemoveRange(this.WebTokens.Where(t => t.UserId == user.UserId));
        this.Comments.RemoveRange(this.Comments.Where(c => c.PosterUserId == user.UserId));
        this.Reviews.RemoveRange(this.Reviews.Where(r => r.ReviewerId == user.UserId));
        this.Photos.RemoveRange(this.Photos.Where(p => p.CreatorId == user.UserId));
        this.Reactions.RemoveRange(this.Reactions.Where(p => p.UserId == user.UserId));

        this.Users.Remove(user);

        await this.SaveChangesAsync();
    }

    public async Task RemoveSlot(Slot slot, bool saveChanges = true)
    {
        if (slot.Location != null) this.Locations.Remove(slot.Location);
        this.Slots.Remove(slot);

        if (saveChanges) await this.SaveChangesAsync();
    }
    #nullable disable
}