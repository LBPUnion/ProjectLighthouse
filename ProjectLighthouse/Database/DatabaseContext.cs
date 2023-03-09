using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Maintenance;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext : DbContext
{
    #region Database entities

    #region Tokens
    // ReSharper disable once InconsistentNaming
    public DbSet<ApiKeyEntity> APIKeys { get; set; }
    public DbSet<EmailSetTokenEntity> EmailSetTokens { get; set; }
    public DbSet<EmailVerificationTokenEntity> EmailVerificationTokens { get; set; }
    public DbSet<GameTokenEntity> GameTokens { get; set; }
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens { get; set; }
    public DbSet<RegistrationTokenEntity> RegistrationTokens { get; set; }
    public DbSet<WebTokenEntity> WebTokens { get; set; }
    #endregion

    #region Users
    public DbSet<CommentEntity> Comments { get; set; }
    public DbSet<LastContactEntity> LastContacts { get; set; }
    public DbSet<PhotoEntity> Photos { get; set; }
    public DbSet<PhotoSubjectEntity> PhotoSubjects { get; set; }
    public DbSet<PlatformLinkAttemptEntity> PlatformLinkAttempts { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    #endregion

    #region Levels
    public DbSet<DatabaseCategoryEntity> CustomCategories { get; set; }
    public DbSet<PlaylistEntity> Playlists { get; set; }
    public DbSet<ReviewEntity> Reviews { get; set; }
    public DbSet<ScoreEntity> Scores { get; set; }
    public DbSet<SlotEntity> Slots { get; set; }
    #endregion

    #region Interactions
    public DbSet<BlockedProfileEntity> BlockedProfiles { get; set; }
    public DbSet<HeartedLevelEntity> HeartedLevels { get; set; }
    public DbSet<HeartedPlaylistEntity> HeartedPlaylists { get; set; }
    public DbSet<HeartedProfileEntity> HeartedProfiles { get; set; }
    public DbSet<QueuedLevelEntity> QueuedLevels { get; set; }
    public DbSet<RatedLevelEntity> RatedLevels { get; set; }
    public DbSet<RatedReview> RatedReviews { get; set; }
    public DbSet<ReactionEntity> Reactions { get; set; }
    public DbSet<VisitedLevelEntity> VisitedLevels { get; set; }
    #endregion

    #region Moderation
    public DbSet<ModerationCaseEntity> Cases { get; set; }
    public DbSet<GriefReport> Reports { get; set; }
    #endregion

    #region Misc
    public DbSet<CompletedMigrationEntity> CompletedMigrations { get; set; }
    #endregion

    #endregion

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseMySql(ServerConfiguration.Instance.DbConnectionString, MySqlServerVersion.LatestSupportedServerVersion);
}