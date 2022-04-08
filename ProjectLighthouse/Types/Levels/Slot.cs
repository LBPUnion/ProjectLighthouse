#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using LBPUnion.ProjectLighthouse.Types.Reviews;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

/// <summary>
///     A LittleBigPlanet level.
/// </summary>
[XmlRoot("slot")]
[XmlType("slot")]
public class Slot
{
    [NotMapped]
    [JsonIgnore]
    [XmlIgnore]
    private Database? _database;

    [NotMapped]
    [JsonIgnore]
    [XmlIgnore]
    private Database database {
        get {
            if (this._database != null) return this._database;

            return this._database = new Database();
        }
        set => this._database = value;
    }

    [XmlAttribute("type")]
    [NotMapped]
    [JsonIgnore]
    public string Type { get; set; } = "user";

    [Key]
    [XmlElement("id")]
    public int SlotId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = "";

    [XmlElement("description")]
    public string Description { get; set; } = "";

    [XmlElement("icon")]
    public string IconHash { get; set; } = "";

    [XmlElement("rootLevel")]
    [JsonIgnore]
    public string RootLevel { get; set; } = "";

    [JsonIgnore]
    public string ResourceCollection { get; set; } = "";

    [NotMapped]
    [XmlElement("resource")]
    [JsonIgnore]
    public string[] Resources {
        get => this.ResourceCollection.Split(",");
        set => this.ResourceCollection = string.Join(',', value);
    }

    [XmlIgnore]
    [JsonIgnore]
    public int LocationId { get; set; }

    [XmlIgnore]
    public int CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    [JsonIgnore]
    public User? Creator { get; set; }

    /// <summary>
    ///     The location of the level on the creator's earth
    /// </summary>
    [XmlElement("location")]
    [ForeignKey(nameof(LocationId))]
    [JsonIgnore]
    public Location? Location { get; set; }

    [XmlElement("initiallyLocked")]
    public bool InitiallyLocked { get; set; }

    [XmlElement("isSubLevel")]
    public bool SubLevel { get; set; }

    [XmlElement("isLBP1Only")]
    public bool Lbp1Only { get; set; }

    [XmlElement("shareable")]
    public int Shareable { get; set; }

    [XmlElement("authorLabels")]
    public string AuthorLabels { get; set; } = "";

    [XmlElement("background")]
    [JsonIgnore]
    public string BackgroundHash { get; set; } = "";

    [XmlElement("minPlayers")]
    public int MinimumPlayers { get; set; }

    [XmlElement("maxPlayers")]
    public int MaximumPlayers { get; set; }

    [XmlElement("moveRequired")]
    public bool MoveRequired { get; set; }

    [XmlIgnore]
    public long FirstUploaded { get; set; }

    [XmlIgnore]
    public long LastUpdated { get; set; }

    [XmlIgnore]
    public bool TeamPick { get; set; }

    [XmlIgnore]
    public GameVersion GameVersion { get; set; }

    [XmlIgnore]
    [NotMapped]
    [JsonIgnore]
    public int Hearts => this.database.HeartedLevels.Count(s => s.SlotId == this.SlotId);

    [XmlIgnore]
    [NotMapped]
    [JsonIgnore]
    public int Comments => this.database.Comments.Count(c => c.Type == CommentType.Level && c.TargetId == this.SlotId);

    [XmlIgnore]
    [NotMapped]
    public int Plays => this.PlaysLBP1 + this.PlaysLBP2 + this.PlaysLBP3 + this.PlaysLBPVita;

    [XmlIgnore]
    [NotMapped]
    public int PlaysUnique => this.PlaysLBP1Unique + this.PlaysLBP2Unique + this.PlaysLBP3Unique + this.PlaysLBPVitaUnique;

    [XmlIgnore]
    [NotMapped]
    public int PlaysComplete => this.PlaysLBP1Complete + this.PlaysLBP2Complete + this.PlaysLBP3Complete + this.PlaysLBPVitaComplete;

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP1 { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP1Complete { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP1Unique { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP2 { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP2Complete { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP2Unique { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP3 { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP3Complete { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBP3Unique { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBPVita { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBPVitaComplete { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    public int PlaysLBPVitaUnique { get; set; }

    [NotMapped]
    [JsonIgnore]
    [XmlElement("thumbsup")]
    public int Thumbsup => this.database.RatedLevels.Count(r => r.SlotId == this.SlotId && r.Rating == 1);

    [NotMapped]
    [JsonIgnore]
    [XmlElement("thumbsdown")]
    public int Thumbsdown => this.database.RatedLevels.Count(r => r.SlotId == this.SlotId && r.Rating == -1);

    [NotMapped]
    [JsonPropertyName("averageRating")]
    [XmlElement("averageRating")]
    public double RatingLBP1 {
        get {
            IQueryable<RatedLevel> ratedLevels = this.database.RatedLevels.Where(r => r.SlotId == this.SlotId && r.RatingLBP1 > 0);
            if (!ratedLevels.Any()) return 3.0;

            return Enumerable.Average(ratedLevels, r => r.RatingLBP1);
        }
    }

    [NotMapped]
    [JsonIgnore]
    [XmlElement("reviewCount")]
    public int ReviewCount => this.database.Reviews.Count(r => r.SlotId == this.SlotId);

    [XmlElement("leveltype")]
    public string LevelType { get; set; } = "";

    [XmlElement("vitaCrossControlRequired")]
    public bool CrossControllerRequired { get; set; }

    public string SerializeResources()
    {
        return this.Resources.Aggregate("", (current, resource) => current + LbpSerializer.StringElement("resource", resource)) +
               LbpSerializer.StringElement("sizeOfResources", this.Resources.Sum(FileHelper.ResourceSize));
    }

    public string Serialize
    (
        GameVersion gameVersion = GameVersion.LittleBigPlanet1,
        RatedLevel? yourRatingStats = null,
        VisitedLevel? yourVisitedStats = null,
        Review? yourReview = null
    )
    {
        int playerCount = RoomHelper.Rooms.Count(r => r.Slot.SlotType == SlotType.User && r.Slot.SlotId == this.SlotId);

        string slotData = LbpSerializer.StringElement("name", this.Name) +
                          LbpSerializer.StringElement("id", this.SlotId) +
                          LbpSerializer.StringElement("game", (int)this.GameVersion) +
                          LbpSerializer.StringElement("npHandle", this.Creator?.Username) +
                          LbpSerializer.StringElement("description", this.Description) +
                          LbpSerializer.StringElement("icon", this.IconHash) +
                          LbpSerializer.StringElement("rootLevel", this.RootLevel) +
                          LbpSerializer.StringElement("authorLabels", this.AuthorLabels) +
                          LbpSerializer.StringElement("labels", this.AuthorLabels) +
                          this.SerializeResources() +
                          LbpSerializer.StringElement("location", this.Location?.Serialize()) +
                          LbpSerializer.StringElement("initiallyLocked", this.InitiallyLocked) +
                          LbpSerializer.StringElement("isSubLevel", this.SubLevel) +
                          LbpSerializer.StringElement("isLBP1Only", this.Lbp1Only) +
                          LbpSerializer.StringElement("shareable", this.Shareable) +
                          LbpSerializer.StringElement("background", this.BackgroundHash) +
                          LbpSerializer.StringElement("minPlayers", this.MinimumPlayers) +
                          LbpSerializer.StringElement("maxPlayers", this.MaximumPlayers) +
                          LbpSerializer.StringElement("moveRequired", this.MoveRequired) +
                          LbpSerializer.StringElement("firstPublished", this.FirstUploaded) +
                          LbpSerializer.StringElement("lastUpdated", this.LastUpdated) +
                          LbpSerializer.StringElement("mmpick", this.TeamPick) +
                          LbpSerializer.StringElement("heartCount", this.Hearts) +
                          LbpSerializer.StringElement("playCount", this.Plays) +
                          LbpSerializer.StringElement("commentCount", this.Comments) +
                          LbpSerializer.StringElement("uniquePlayCount", this.PlaysLBP2Unique) + // ??? good naming scheme lol
                          LbpSerializer.StringElement("completionCount", this.PlaysComplete) +
                          LbpSerializer.StringElement("lbp1PlayCount", this.PlaysLBP1) +
                          LbpSerializer.StringElement("lbp1CompletionCount", this.PlaysLBP1Complete) +
                          LbpSerializer.StringElement("lbp1UniquePlayCount", this.PlaysLBP1Unique) +
                          LbpSerializer.StringElement("lbp3PlayCount", this.PlaysLBP3) +
                          LbpSerializer.StringElement("lbp3CompletionCount", this.PlaysLBP3Complete) +
                          LbpSerializer.StringElement("lbp3UniquePlayCount", this.PlaysLBP3Unique) +
                          LbpSerializer.StringElement("thumbsup", this.Thumbsup) +
                          LbpSerializer.StringElement("thumbsdown", this.Thumbsdown) +
                          LbpSerializer.StringElement("averageRating", this.RatingLBP1) +
                          LbpSerializer.StringElement("leveltype", this.LevelType) +
                          LbpSerializer.StringElement("yourRating", yourRatingStats?.RatingLBP1) +
                          LbpSerializer.StringElement("yourDPadRating", yourRatingStats?.Rating) +
                          LbpSerializer.StringElement("yourlbpPlayCount", yourVisitedStats?.PlaysLBP1) +
                          LbpSerializer.StringElement("yourlbp3PlayCount", yourVisitedStats?.PlaysLBP3) +
                          yourReview?.Serialize("yourReview") +
                          LbpSerializer.StringElement("reviewsEnabled", ServerSettings.Instance.LevelReviewsEnabled) +
                          LbpSerializer.StringElement("commentsEnabled", ServerSettings.Instance.LevelCommentsEnabled) +
                          LbpSerializer.StringElement("playerCount", playerCount) +
                          LbpSerializer.StringElement("reviewCount", this.ReviewCount);

        int yourPlays;
        int plays;
        int playsComplete;
        int playsUnique;

        if (gameVersion == GameVersion.LittleBigPlanetVita)
        {
            yourPlays = yourVisitedStats?.PlaysLBPVita ?? 0;
            plays = this.PlaysLBPVita;
            playsComplete = this.PlaysLBPVitaComplete;
            playsUnique = this.PlaysLBPVitaUnique;
        }
        else
        {
            yourPlays = yourVisitedStats?.PlaysLBP2 ?? 0;
            plays = this.PlaysLBP2;
            playsComplete = this.PlaysLBP2Complete;
            playsUnique = this.PlaysLBP2Unique;
        }

        slotData += LbpSerializer.StringElement("yourlbp2PlayCount", yourPlays) +
                    LbpSerializer.StringElement("lbp2PlayCount", plays) +
                    LbpSerializer.StringElement("playCount", plays) +
                    LbpSerializer.StringElement("lbp2CompletionCount", playsComplete) +
                    LbpSerializer.StringElement("completionCount", playsComplete) +
                    LbpSerializer.StringElement("lbp2UniquePlayCount", playsUnique) + // not actually used ingame, as per above comment
                    LbpSerializer.StringElement("uniquePlayCount", playsUnique);

        return LbpSerializer.TaggedStringElement("slot", slotData, "type", "user");
    }
}