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
    public virtual DbSet<ApiKeyEntity> APIKeys { get; set; }
    public virtual DbSet<EmailSetTokenEntity> EmailSetTokens { get; set; }
    public virtual DbSet<EmailVerificationTokenEntity> EmailVerificationTokens { get; set; }
    public virtual DbSet<GameTokenEntity> GameTokens { get; set; }
    public virtual DbSet<PasswordResetTokenEntity> PasswordResetTokens { get; set; }
    public virtual DbSet<RegistrationTokenEntity> RegistrationTokens { get; set; }
    public virtual DbSet<WebTokenEntity> WebTokens { get; set; }
    #endregion

    #region Users
    public virtual DbSet<CommentEntity> Comments { get; set; }
    public virtual DbSet<LastContactEntity> LastContacts { get; set; }
    public virtual DbSet<PhotoEntity> Photos { get; set; }
    public virtual DbSet<PhotoSubjectEntity> PhotoSubjects { get; set; }
    public virtual DbSet<PlatformLinkAttemptEntity> PlatformLinkAttempts { get; set; }
    public virtual DbSet<UserEntity> Users { get; set; }
    #endregion

    #region Levels
    public virtual DbSet<DatabaseCategoryEntity> CustomCategories { get; set; }
    public virtual DbSet<PlaylistEntity> Playlists { get; set; }
    public virtual DbSet<ReviewEntity> Reviews { get; set; }
    public virtual DbSet<ScoreEntity> Scores { get; set; }
    public virtual DbSet<SlotEntity> Slots { get; set; }
    #endregion

    #region Interactions
    public virtual DbSet<BlockedProfileEntity> BlockedProfiles { get; set; }
    public virtual DbSet<HeartedLevelEntity> HeartedLevels { get; set; }
    public virtual DbSet<HeartedPlaylistEntity> HeartedPlaylists { get; set; }
    public virtual DbSet<HeartedProfileEntity> HeartedProfiles { get; set; }
    public virtual DbSet<QueuedLevelEntity> QueuedLevels { get; set; }
    public virtual DbSet<RatedCommentEntity> RatedComments { get; set; }
    public virtual DbSet<RatedLevelEntity> RatedLevels { get; set; }
    public virtual DbSet<RatedReviewEntity> RatedReviews { get; set; }
    public virtual DbSet<VisitedLevelEntity> VisitedLevels { get; set; }
    #endregion

    #region Moderation
    public virtual DbSet<ModerationCaseEntity> Cases { get; set; }
    public virtual DbSet<GriefReportEntity> Reports { get; set; }
    #endregion

    #region Misc
    public virtual DbSet<CompletedMigrationEntity> CompletedMigrations { get; set; }
    #endregion

    #endregion

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    { }

    public static DatabaseContext CreateNewInstance()
    {
        DbContextOptionsBuilder<DatabaseContext> builder = new();
        builder.UseMySql(ServerConfiguration.Instance.DbConnectionString,
            MySqlServerVersion.LatestSupportedServerVersion);
        return new DatabaseContext(builder.Options);
    }
}