using System;
using LBPUnion.ProjectLighthouse;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectLighthouse.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20220610230341_AddModerationCases")]
    public class AddModerationCases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompletedMigrations",
                columns: table => new
                {
                    MigrationName = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RanAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedMigrations", x => x.MigrationName);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomCategories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Endpoint = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SlotIdsCollection = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomCategories", x => x.CategoryId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reactions",
                columns: table => new
                {
                    RatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reactions", x => x.RatingId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WebTokens",
                columns: table => new
                {
                    TokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebTokens", x => x.TokenId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailAddressVerified = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Password = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Game = table.Column<int>(type: "int", nullable: false),
                    Biography = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    Pins = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanetHashLBP2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanetHashLBP3 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanetHashLBPVita = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordResetRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    YayHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BooHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MehHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PermissionLevel = table.Column<int>(type: "int", nullable: false),
                    BannedReason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdminGrantedSlots = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    CaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CaseType = table.Column<int>(type: "int", nullable: false),
                    CaseDescription = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CaseCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CaseCreatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.CaseId);
                    table.ForeignKey(
                        name: "FK_Cases_Users_CaseCreatorId",
                        column: x => x.CaseCreatorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PosterUserId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ThumbsUp = table.Column<int>(type: "int", nullable: false),
                    ThumbsDown = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_Users_PosterUserId",
                        column: x => x.PosterUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailSetTokens",
                columns: table => new
                {
                    EmailSetTokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmailToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSetTokens", x => x.EmailSetTokenId);
                    table.ForeignKey(
                        name: "FK_EmailSetTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmailVerificationTokens",
                columns: table => new
                {
                    EmailVerificationTokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmailToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationTokens", x => x.EmailVerificationTokenId);
                    table.ForeignKey(
                        name: "FK_EmailVerificationTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "GameTokens",
                columns: table => new
                {
                    TokenId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserLocation = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GameVersion = table.Column<int>(type: "int", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    Approved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Used = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameTokens", x => x.TokenId);
                    table.ForeignKey(
                        name: "FK_GameTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HeartedProfiles",
                columns: table => new
                {
                    HeartedProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    HeartedUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeartedProfiles", x => x.HeartedProfileId);
                    table.ForeignKey(
                        name: "FK_HeartedProfiles_Users_HeartedUserId",
                        column: x => x.HeartedUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HeartedProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LastContacts",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    GameVersion = table.Column<int>(type: "int", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LastContacts", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_LastContacts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    SmallHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MediumHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LargeHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlanHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoSubjectCollection = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.PhotoId);
                    table.ForeignKey(
                        name: "FK_Photos_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhotoSubjects",
                columns: table => new
                {
                    PhotoSubjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Bounds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoSubjects", x => x.PhotoSubjectId);
                    table.ForeignKey(
                        name: "FK_PhotoSubjects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    ReportingPlayerId = table.Column<int>(type: "int", nullable: false),
                    Players = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GriefStateHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LevelOwner = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InitialStateHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JpegHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LevelId = table.Column<int>(type: "int", nullable: false),
                    LevelType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bounds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReportingPlayerId",
                        column: x => x.ReportingPlayerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    SlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IconHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RootLevel = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResourceCollection = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: false),
                    InitiallyLocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SubLevel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Lbp1Only = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Shareable = table.Column<int>(type: "int", nullable: false),
                    AuthorLabels = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BackgroundHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinimumPlayers = table.Column<int>(type: "int", nullable: false),
                    MaximumPlayers = table.Column<int>(type: "int", nullable: false),
                    MoveRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FirstUploaded = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdated = table.Column<long>(type: "bigint", nullable: false),
                    TeamPick = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GameVersion = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP1 = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP1Complete = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP1Unique = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP2 = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP2Complete = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP2Unique = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP3 = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP3Complete = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP3Unique = table.Column<int>(type: "int", nullable: false),
                    PlaysLBPVita = table.Column<int>(type: "int", nullable: false),
                    PlaysLBPVitaComplete = table.Column<int>(type: "int", nullable: false),
                    PlaysLBPVitaUnique = table.Column<int>(type: "int", nullable: false),
                    LevelType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CrossControllerRequired = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.SlotId);
                    table.ForeignKey(
                        name: "FK_Slots_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Slots_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserApprovedIpAddresses",
                columns: table => new
                {
                    UserApprovedIpAddressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApprovedIpAddresses", x => x.UserApprovedIpAddressId);
                    table.ForeignKey(
                        name: "FK_UserApprovedIpAddresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuthenticationAttempts",
                columns: table => new
                {
                    AuthenticationAttemptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    Platform = table.Column<int>(type: "int", nullable: false),
                    IPAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GameTokenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationAttempts", x => x.AuthenticationAttemptId);
                    table.ForeignKey(
                        name: "FK_AuthenticationAttempts_GameTokens_GameTokenId",
                        column: x => x.GameTokenId,
                        principalTable: "GameTokens",
                        principalColumn: "TokenId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HeartedLevels",
                columns: table => new
                {
                    HeartedLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeartedLevels", x => x.HeartedLevelId);
                    table.ForeignKey(
                        name: "FK_HeartedLevels_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HeartedLevels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "QueuedLevels",
                columns: table => new
                {
                    QueuedLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedLevels", x => x.QueuedLevelId);
                    table.ForeignKey(
                        name: "FK_QueuedLevels_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QueuedLevels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RatedLevels",
                columns: table => new
                {
                    RatedLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    RatingLBP1 = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatedLevels", x => x.RatedLevelId);
                    table.ForeignKey(
                        name: "FK_RatedLevels_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RatedLevels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    ReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<long>(type: "bigint", nullable: false),
                    LabelCollection = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Deleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeletedBy = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Thumb = table.Column<int>(type: "int", nullable: false),
                    ThumbsUp = table.Column<int>(type: "int", nullable: false),
                    ThumbsDown = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.ReviewId);
                    table.ForeignKey(
                        name: "FK_Reviews_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Scores",
                columns: table => new
                {
                    ScoreId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    PlayerIdCollection = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Points = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scores", x => x.ScoreId);
                    table.ForeignKey(
                        name: "FK_Scores_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VisitedLevels",
                columns: table => new
                {
                    VisitedLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SlotId = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP1 = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP2 = table.Column<int>(type: "int", nullable: false),
                    PlaysLBP3 = table.Column<int>(type: "int", nullable: false),
                    PlaysLBPVita = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitedLevels", x => x.VisitedLevelId);
                    table.ForeignKey(
                        name: "FK_VisitedLevels_Slots_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slots",
                        principalColumn: "SlotId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VisitedLevels_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RatedReviews",
                columns: table => new
                {
                    RatedReviewId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    Thumb = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatedReviews", x => x.RatedReviewId);
                    table.ForeignKey(
                        name: "FK_RatedReviews_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "ReviewId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RatedReviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationAttempts_GameTokenId",
                table: "AuthenticationAttempts",
                column: "GameTokenId");

            migrationBuilder.CreateIndex(
                name: "IX_Cases_CaseCreatorId",
                table: "Cases",
                column: "CaseCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_PosterUserId",
                table: "Comments",
                column: "PosterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSetTokens_UserId",
                table: "EmailSetTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_UserId",
                table: "EmailVerificationTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameTokens_UserId",
                table: "GameTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeartedLevels_SlotId",
                table: "HeartedLevels",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_HeartedLevels_UserId",
                table: "HeartedLevels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeartedProfiles_HeartedUserId",
                table: "HeartedProfiles",
                column: "HeartedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeartedProfiles_UserId",
                table: "HeartedProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_CreatorId",
                table: "Photos",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoSubjects_UserId",
                table: "PhotoSubjects",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedLevels_SlotId",
                table: "QueuedLevels",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedLevels_UserId",
                table: "QueuedLevels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RatedLevels_SlotId",
                table: "RatedLevels",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_RatedLevels_UserId",
                table: "RatedLevels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RatedReviews_ReviewId",
                table: "RatedReviews",
                column: "ReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_RatedReviews_UserId",
                table: "RatedReviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportingPlayerId",
                table: "Reports",
                column: "ReportingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReviewerId",
                table: "Reviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SlotId",
                table: "Reviews",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Scores_SlotId",
                table: "Scores",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_CreatorId",
                table: "Slots",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_LocationId",
                table: "Slots",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApprovedIpAddresses_UserId",
                table: "UserApprovedIpAddresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LocationId",
                table: "Users",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitedLevels_SlotId",
                table: "VisitedLevels",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitedLevels_UserId",
                table: "VisitedLevels",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationAttempts");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CompletedMigrations");

            migrationBuilder.DropTable(
                name: "CustomCategories");

            migrationBuilder.DropTable(
                name: "EmailSetTokens");

            migrationBuilder.DropTable(
                name: "EmailVerificationTokens");

            migrationBuilder.DropTable(
                name: "HeartedLevels");

            migrationBuilder.DropTable(
                name: "HeartedProfiles");

            migrationBuilder.DropTable(
                name: "LastContacts");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "PhotoSubjects");

            migrationBuilder.DropTable(
                name: "QueuedLevels");

            migrationBuilder.DropTable(
                name: "RatedLevels");

            migrationBuilder.DropTable(
                name: "RatedReviews");

            migrationBuilder.DropTable(
                name: "Reactions");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Scores");

            migrationBuilder.DropTable(
                name: "UserApprovedIpAddresses");

            migrationBuilder.DropTable(
                name: "VisitedLevels");

            migrationBuilder.DropTable(
                name: "WebTokens");

            migrationBuilder.DropTable(
                name: "GameTokens");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Slots");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
