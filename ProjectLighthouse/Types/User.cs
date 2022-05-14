using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Types;

public class User
{
    #nullable enable
    [NotMapped]
    [JsonIgnore]
    private Database? _database;
    #nullable disable

    [NotMapped]
    [JsonIgnore]
    private Database database {
        get {
            if (this._database != null) return this._database;

            return this._database = new Database();
        }
        set => this._database = value;
    }

    public int UserId { get; set; }
    public string Username { get; set; }

    #nullable enable
    [JsonIgnore]
    public string? EmailAddress { get; set; } = null;
    #nullable disable

    public bool EmailAddressVerified { get; set; } = false;

    [JsonIgnore]
    public string Password { get; set; }

    public string IconHash { get; set; }

    [JsonIgnore]
    public int Game { get; set; }

    [NotMapped]
    [JsonIgnore]
    public int Lists => 0;

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
    public int Reviews => database.Reviews.Count(r => r.ReviewerId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int Comments => database.Comments.Count(c => c.Type == CommentType.Profile && c.TargetId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int PhotosByMe => database.Photos.Count(p => p.CreatorId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int PhotosWithMe => Enumerable.Sum(database.Photos, photo => photo.Subjects.Count(subject => subject.User.UserId == this.UserId));

    [JsonIgnore]
    public int LocationId { get; set; }

    /// <summary>
    ///     The location of the profile card on the user's earth
    /// </summary>
    [ForeignKey("LocationId")]
    [JsonIgnore]
    public Location Location { get; set; }

    [NotMapped]
    [JsonIgnore]
    public int HeartedLevels => database.HeartedLevels.Count(p => p.UserId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int HeartedUsers => database.HeartedProfiles.Count(p => p.UserId == this.UserId);

    [NotMapped]
    [JsonIgnore]
    public int QueuedLevels => database.QueuedLevels.Count(p => p.UserId == this.UserId);

    [JsonIgnore]
    public string Pins { get; set; } = "";

    [JsonIgnore]
    public string PlanetHashLBP2 { get; set; } = "";

    [JsonIgnore]
    public string PlanetHashLBP3 { get; set; } = "";

    [JsonIgnore]
    public string PlanetHashLBPVita { get; set; } = "";

    [JsonIgnore]
    public int Hearts => database.HeartedProfiles.Count(s => s.HeartedUserId == this.UserId);

    [JsonIgnore]
    public bool IsAdmin { get; set; } = false;

    [JsonIgnore]
    public bool PasswordResetRequired { get; set; }

    public string YayHash { get; set; } = "";
    public string BooHash { get; set; } = "";
    public string MehHash { get; set; } = "";

    [NotMapped]
    [JsonIgnore]
    public UserStatus Status => new(database, this.UserId);

    [JsonIgnore]
    public bool Banned { get; set; }

    [JsonIgnore]
    public string BannedReason { get; set; }

    public string Serialize(GameVersion gameVersion = GameVersion.LittleBigPlanet1)
    {
        string user = LbpSerializer.TaggedStringElement("npHandle", this.Username, "icon", this.IconHash) +
                      LbpSerializer.StringElement("game", (int)gameVersion) +
                      this.serializeSlots(gameVersion) +
                      LbpSerializer.StringElement("lists", this.Lists) +
                      LbpSerializer.StringElement
                      (
                          "lists_quota",
                          ServerConfiguration.Instance.UserGeneratedContentLimits.ListsQuota
                      ) + // technically not a part of the user but LBP expects it
                      LbpSerializer.StringElement("biography", this.Biography) +
                      LbpSerializer.StringElement("reviewCount", this.Reviews) +
                      LbpSerializer.StringElement("commentCount", this.Comments) +
                      LbpSerializer.StringElement("photosByMeCount", this.PhotosByMe) +
                      LbpSerializer.StringElement("photosWithMeCount", this.PhotosWithMe) +
                      LbpSerializer.StringElement("commentsEnabled", ServerConfiguration.Instance.UserGeneratedContentLimits.ProfileCommentsEnabled) +
                      LbpSerializer.StringElement("location", this.Location.Serialize()) +
                      LbpSerializer.StringElement("favouriteSlotCount", this.HeartedLevels) +
                      LbpSerializer.StringElement("favouriteUserCount", this.HeartedUsers) +
                      LbpSerializer.StringElement("lolcatftwCount", this.QueuedLevels) +
                      LbpSerializer.StringElement("pins", this.Pins) +
                      serializeEarth(gameVersion) +
                      LbpSerializer.BlankElement("photos") +
                      LbpSerializer.StringElement("heartCount", this.Hearts) +
                      LbpSerializer.StringElement("yay2", this.YayHash) +
                      LbpSerializer.StringElement("boo2", this.BooHash) +
                      LbpSerializer.StringElement("meh2", this.MehHash);

        return LbpSerializer.TaggedStringElement("user", user, "type", "user");
    }

    private string serializeEarth(GameVersion gameVersion)
    {
        return LbpSerializer.StringElement
        (
            "planets",
            gameVersion switch
            {
                GameVersion.LittleBigPlanet2 => this.PlanetHashLBP2,
                GameVersion.LittleBigPlanet3 => this.PlanetHashLBP3,
                GameVersion.LittleBigPlanetVita => this.PlanetHashLBPVita,
                _ => "", // other versions do not have custom planets
            }
        );
    }

    #region Slots

    /// <summary>
    ///     The number of used slots on the earth
    /// </summary>
    [NotMapped]
    [JsonIgnore]
    public int UsedSlots => database.Slots.Count(s => s.CreatorId == this.UserId);

    #nullable enable
    public int GetUsedSlotsForGame(GameVersion version)
    {
        return database.Slots.Count(s => s.CreatorId == this.UserId && s.GameVersion == version);
    }
    #nullable disable

    [JsonIgnore]
    [XmlIgnore]
    public int EntitledSlots => ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots + this.AdminGrantedSlots;

    /// <summary>
    ///     The number of slots remaining on the earth
    /// </summary>
    [JsonIgnore]
    public int FreeSlots => this.EntitledSlots - this.UsedSlots;

    [JsonIgnore]
    [XmlIgnore]
    public int AdminGrantedSlots { get; set; }

    private static readonly string[] slotTypes =
    {
        "lbp2", "lbp3", "crossControl",
    };

    private string serializeSlots(GameVersion gameVersion)
    {
        string slots = string.Empty;

        string[] slotTypesLocal;

        if (gameVersion == GameVersion.LittleBigPlanetVita)
        {
            slots += LbpSerializer.StringElement("lbp2UsedSlots", this.GetUsedSlotsForGame(GameVersion.LittleBigPlanetVita));
            slotTypesLocal = new[]
            {
                "lbp2",
            };
        }
        else
        {
            slots += LbpSerializer.StringElement("lbp1UsedSlots", this.GetUsedSlotsForGame(GameVersion.LittleBigPlanet1));
            slots += LbpSerializer.StringElement("lbp2UsedSlots", this.GetUsedSlotsForGame(GameVersion.LittleBigPlanet2));
            slots += LbpSerializer.StringElement("lbp3UsedSlots", this.GetUsedSlotsForGame(GameVersion.LittleBigPlanet3));
            slotTypesLocal = slotTypes;
        }

        slots += LbpSerializer.StringElement("entitledSlots", this.EntitledSlots);
        slots += LbpSerializer.StringElement("freeSlots", this.FreeSlots);

        foreach (string slotType in slotTypesLocal)
        {
            slots += LbpSerializer.StringElement(slotType + "EntitledSlots", this.EntitledSlots);
            // ReSharper disable once StringLiteralTypo
            slots += LbpSerializer.StringElement(slotType + slotType == "crossControl" ? "PurchsedSlots" : "PurchasedSlots", 0);
            slots += LbpSerializer.StringElement(slotType + "FreeSlots", this.FreeSlots);
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