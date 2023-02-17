using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Maintenance;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Misc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext : DbContext
{
    #region Database entities

    #region Tokens
    // ReSharper disable once InconsistentNaming
    public DbSet<ApiKey> APIKeys { get; set; }
    public DbSet<EmailSetToken> EmailSetTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    public DbSet<GameToken> GameTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<RegistrationToken> RegistrationTokens { get; set; }
    public DbSet<WebToken> WebTokens { get; set; }
    #endregion

    #region Users
    public DbSet<Comment> Comments { get; set; }
    public DbSet<LastContact> LastContacts { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<PhotoSubject> PhotoSubjects { get; set; }
    public DbSet<PlatformLinkAttempt> PlatformLinkAttempts { get; set; }
    public DbSet<User> Users { get; set; }
    #endregion

    #region Levels
    public DbSet<DatabaseCategory> CustomCategories { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<Slot> Slots { get; set; }
    #endregion

    #region Interactions
    public DbSet<BlockedProfile> BlockedProfiles { get; set; }
    public DbSet<HeartedLevel> HeartedLevels { get; set; }
    public DbSet<HeartedPlaylist> HeartedPlaylists { get; set; }
    public DbSet<HeartedProfile> HeartedProfiles { get; set; }
    public DbSet<QueuedLevel> QueuedLevels { get; set; }
    public DbSet<RatedLevel> RatedLevels { get; set; }
    public DbSet<RatedReview> RatedReviews { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<VisitedLevel> VisitedLevels { get; set; }
    #endregion

    #region Moderation
    public DbSet<ModerationCase> Cases { get; set; }
    public DbSet<GriefReport> Reports { get; set; }
    #endregion

    #region Misc
    public DbSet<CompletedMigration> CompletedMigrations { get; set; }
    #endregion

    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseMySql(ServerConfiguration.Instance.DbConnectionString, MySqlServerVersion.LatestSupportedServerVersion);
}
