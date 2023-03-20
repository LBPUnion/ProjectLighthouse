using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

public class User
{
    #nullable enable
    [NotMapped]
    [JsonIgnore]
    private DatabaseContext? _database;

    [NotMapped]
    [JsonIgnore]
    private DatabaseContext database {
        get {
            if (this._database != null) return this._database;

            return this._database = new DatabaseContext();
        }
        set => this._database = value;
    }

    public int UserId { get; set; }
    public string Username { get; set; } = "";

    #nullable enable
    [JsonIgnore]
    public string? EmailAddress { get; set; }
    #nullable disable

    public bool EmailAddressVerified { get; set; }

    [JsonIgnore]
    public string Password { get; set; }

    public string IconHash { get; set; }

    [JsonIgnore]
    public int Game { get; set; }

    [NotMapped]
    [JsonIgnore]
    public int Lists => this.database.Playlists.Count(p => p.CreatorId == this.UserId);

    /// <summary>
    ///     A user-customizable biography shown on the profile card
    /// </summary>
    public string Biography { get; set; }

    [NotMapped]
    [JsonIgnore]
    public string WebsiteAvatarHash {
        get {
            string avatarHash = this.IconHash;

            if (string.IsNullOrWhiteSpace(avatarHash) || this.IconHash.StartsWith('g')) avatarHash = this.YayHash;
            if (string.IsNullOrWhiteSpace(avatarHash)) avatarHash = this.MehHash;
            if (string.IsNullOrWhiteSpace(avatarHash)) avatarHash = this.BooHash;
            if (string.IsNullOrWhiteSpace(avatarHash)) avatarHash = ServerConfiguration.Instance.WebsiteConfiguration.MissingIconHash;

            return avatarHash;
        }
    }

    [NotMapped]
    [JsonIgnore]
    public int Reviews => this.database.Reviews.Count(r => r.ReviewerId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int Comments => this.database.Comments.Count(c => c.Type == CommentType.Profile && c.TargetId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int PhotosByMe => this.database.Photos.Count(p => p.CreatorId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int PhotosWithMe => this.database.Photos.Include(p => p.PhotoSubjects).Count(p => p.PhotoSubjects.Any(ps => ps.UserId == this.UserId));

    /// <summary>
    ///     The location of the profile card on the user's earth
    ///     Stored as a single 64 bit unsigned integer but split into
    ///     2 unsigned 32 bit integers
    /// </summary>
    [JsonIgnore]
    public ulong LocationPacked { get; set; }

    [NotMapped]
    [XmlElement("location")]
    public Location Location
    {
        get =>
            new()
            {
                X = (int)(this.LocationPacked >> 32),
                Y = (int)this.LocationPacked,
            };
        set => this.LocationPacked = (ulong)value.X << 32 | (uint)value.Y;
    }

    [NotMapped]
    [JsonIgnore]
    public int HeartedLevels => this.database.HeartedLevels.Count(p => p.UserId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int HeartedUsers => this.database.HeartedProfiles.Count(p => p.UserId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int HeartedPlaylists => this.database.HeartedPlaylists.Count(p => p.UserId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int QueuedLevels => this.database.QueuedLevels.Count(p => p.UserId == this.UserId);

    [JsonIgnore]
    public string Pins { get; set; } = "";

    [JsonIgnore]
    public string PlanetHashLBP2 { get; set; } = "";

    [JsonIgnore]
    public string PlanetHashLBP2CC { get; set; } = "";

    [JsonIgnore]
    public string PlanetHashLBP3 { get; set; } = "";

    [JsonIgnore]
    public string PlanetHashLBPVita { get; set; } = "";

    [JsonIgnore]
    public int Hearts => this.database.HeartedProfiles.Count(s => s.HeartedUserId == this.UserId);

    [JsonIgnore]
    public bool PasswordResetRequired { get; set; }

    public string YayHash { get; set; } = "";
    public string BooHash { get; set; } = "";
    public string MehHash { get; set; } = "";

    public long LastLogin { get; set; }
    public long LastLogout { get; set; }

    [NotMapped]
    [JsonIgnore]
    public UserStatus Status => new(this.database, this.UserId);

    [JsonIgnore]
    public bool IsBanned => this.PermissionLevel is PermissionLevel.Banned;

    [JsonIgnore]
    public bool IsRestricted => this.PermissionLevel is PermissionLevel.Restricted or PermissionLevel.Banned;

    [JsonIgnore]
    public bool IsSilenced => this.PermissionLevel is PermissionLevel.Silenced or PermissionLevel.Restricted or PermissionLevel.Banned;

    [JsonIgnore]
    public bool IsModerator => this.PermissionLevel is PermissionLevel.Moderator or PermissionLevel.Administrator;

    [JsonIgnore]
    public bool IsAdmin => this.PermissionLevel is PermissionLevel.Administrator;

    [JsonIgnore]
    public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.Default;

    #nullable enable
    [JsonIgnore]
    public string? BannedReason { get; set; }
    #nullable disable

    [JsonIgnore]
    public string Language { get; set; } = "en";

    [JsonIgnore]
    public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;

    public PrivacyType LevelVisibility { get; set; } = PrivacyType.All;

    public PrivacyType ProfileVisibility { get; set; } = PrivacyType.All;

    [JsonIgnore]
    public bool TwoFactorRequired => ServerConfiguration.Instance.TwoFactorConfiguration.RequireTwoFactor && 
                                     this.PermissionLevel >= ServerConfiguration.Instance.TwoFactorConfiguration.RequiredTwoFactorLevel;

    [JsonIgnore]
    public bool IsTwoFactorSetup => this.TwoFactorBackup?.Length > 0 && this.TwoFactorSecret?.Length > 0;

    [JsonIgnore]
    public string TwoFactorSecret { get; set; } = "";

    [JsonIgnore]
    public string TwoFactorBackup { get; set; } = "";

    [JsonIgnore]
    public ulong LinkedRpcnId { get; set; }

    [JsonIgnore]
    public ulong LinkedPsnId { get; set; }

    // should not be adjustable by user
    public bool CommentsEnabled { get; set; } = true;

    public string Serialize(GameVersion gameVersion = GameVersion.LittleBigPlanet1)
    {
        string user = LbpSerializer.TaggedStringElement("npHandle", this.Username, "icon", this.IconHash) +
                      LbpSerializer.StringElement("game", (int)gameVersion) +
                      this.serializeSlots(gameVersion) +
                      LbpSerializer.StringElement<int>("lists", this.Lists, true) +
                      LbpSerializer.StringElement<int>
                      (
                          "lists_quota",
                          ServerConfiguration.Instance.UserGeneratedContentLimits.ListsQuota,
                          true
                      ) + // technically not a part of the user but LBP expects it
                      LbpSerializer.StringElement<int>("heartCount", this.Hearts, true) +
                      this.serializeEarth(gameVersion) +
                      LbpSerializer.StringElement<string>("yay2", this.YayHash, true) +
                      LbpSerializer.StringElement<string>("boo2", this.BooHash, true) +
                      LbpSerializer.StringElement<string>("meh2", this.MehHash, true) +
                      LbpSerializer.StringElement<string>("biography", this.Biography, true) +
                      LbpSerializer.StringElement<int>("reviewCount", this.Reviews, true) +
                      LbpSerializer.StringElement<int>("commentCount", this.Comments, true) +
                      LbpSerializer.StringElement<int>("photosByMeCount", this.PhotosByMe, true) +
                      LbpSerializer.StringElement<int>("photosWithMeCount", this.PhotosWithMe, true) +
                      LbpSerializer.StringElement("commentsEnabled", ServerConfiguration.Instance.UserGeneratedContentLimits.ProfileCommentsEnabled && this.CommentsEnabled) +
                      LbpSerializer.StringElement("location", this.Location.Serialize()) +
                      LbpSerializer.StringElement<int>("favouriteSlotCount", this.HeartedLevels, true) +
                      LbpSerializer.StringElement<int>("favouriteUserCount", this.HeartedUsers, true) +
                      LbpSerializer.StringElement<int>("favouritePlaylistCount", this.HeartedPlaylists, true) +
                      LbpSerializer.StringElement<int>("lolcatftwCount", this.QueuedLevels, true) +
                      LbpSerializer.StringElement<string>("pins", this.Pins, true);

        return LbpSerializer.TaggedStringElement("user", user, "type", "user");
    }

    private string serializeEarth(GameVersion gameVersion)
    {
        string planets = LbpSerializer.StringElement<string>
        (
            "planets",
            gameVersion switch
            {
                GameVersion.LittleBigPlanet2 => this.PlanetHashLBP2,
                GameVersion.LittleBigPlanet3 => this.PlanetHashLBP3,
                GameVersion.LittleBigPlanetVita => this.PlanetHashLBPVita,
                _ => "", // other versions do not have custom planets
            },
            true
        );
        if (gameVersion == GameVersion.LittleBigPlanet2)
            planets += LbpSerializer.StringElement<string>("crossControlPlanet", this.PlanetHashLBP2CC, true);

        return planets;
    }

    #region Slots

    /// <summary>
    ///     The number of used slots on the earth
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public int UsedSlots => this.database.Slots.Count(s => s.CreatorId == this.UserId);

    public int GetUsedSlotsForGame(GameVersion version)
    {
        return this.database.Slots.Count(s => s.CreatorId == this.UserId && s.GameVersion == version);
    }

    [JsonIgnore]
    [XmlIgnore]
    public int EntitledSlots => ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots + this.AdminGrantedSlots;

    [JsonIgnore]
    [XmlIgnore]
    private int CrossControlSlots => this.database.Slots.Count(s => s.CreatorId == this.UserId && s.CrossControllerRequired);

    [JsonIgnore]
    [XmlIgnore]
    public int AdminGrantedSlots { get; set; }

    private string serializeSlots(GameVersion gameVersion)
    {
        string slots = string.Empty;
        int entitledSlots = this.EntitledSlots;
        Dictionary<string, int> usedSlots = new();

        if (gameVersion == GameVersion.LittleBigPlanetVita)
        {
            usedSlots.Add("lbp2", this.GetUsedSlotsForGame(GameVersion.LittleBigPlanetVita));
        }
        else
        {
            int lbp1Used = this.GetUsedSlotsForGame(GameVersion.LittleBigPlanet1);
            int lbp2Used = this.GetUsedSlotsForGame(GameVersion.LittleBigPlanet2);
            int lbp3Used = this.GetUsedSlotsForGame(GameVersion.LittleBigPlanet3);
            int crossControlUsed = this.CrossControlSlots;
            usedSlots.Add("crossControl", crossControlUsed);
            usedSlots.Add("lbp2", lbp2Used);
            usedSlots.Add("lbp3", lbp3Used);
            // these 3 actually correspond to lbp1 only despite the name
            slots += LbpSerializer.StringElement("lbp1UsedSlots", lbp1Used);
            slots += LbpSerializer.StringElement("entitledSlots", entitledSlots);
            slots += LbpSerializer.StringElement("freeSlots", entitledSlots - lbp1Used);
        }

        foreach (KeyValuePair<string, int> entry in usedSlots)
        {
            slots += LbpSerializer.StringElement(entry.Key + "UsedSlots", entry.Value);
            slots += LbpSerializer.StringElement(entry.Key + "EntitledSlots", entitledSlots);
            slots += LbpSerializer.StringElement(entry.Key + (entry.Key == "crossControl" ? "PurchsedSlots" : "PurchasedSlots"), 0);
            slots += LbpSerializer.StringElement(entry.Key + "FreeSlots", entitledSlots - entry.Value);
        }
        return slots;

    }

    #endregion Slots

    #nullable enable
    public override bool Equals(object? obj)
    {
        if (obj is User user) return user.UserId == this.UserId;

        return false;
    }

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    public static bool operator ==(User? user1, User? user2)
    {
        if (ReferenceEquals(user1, user2)) return true;
        if ((object?)user1 == null || (object?)user2 == null) return false;

        return user1.UserId == user2.UserId;
    }
    public static bool operator !=(User? user1, User? user2) => !(user1 == user2);

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode() => this.UserId;
    #nullable disable
}