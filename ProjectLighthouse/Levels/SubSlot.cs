#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Match.Rooms;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData.Reviews;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;

namespace LBPUnion.ProjectLighthouse.Levels;

// Placeholder for a proper Adventure Slot system, sit tight!

/// <summary>
///     A LittleBigPlanet level.
/// </summary>
[XmlRoot("slot")]
[XmlType("advslot")]
public class SubSlot
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
    [JsonIgnore]
    public SlotType Type { get; set; } = SlotType.User;

    [Key]
    [XmlElement("id")]
    public int SlotId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = "";

    [XmlElement("description")]
    public string Description { get; set; } = "";

    [XmlElement("rootLevel")]
    [JsonIgnore]
    public string RootLevel { get; set; } = "";

    [XmlIgnore]
    [JsonIgnore]
    public int LocationId { get; set; }

    /// <summary>
    ///     The location of the level on the creator's earth
    /// </summary>
    [XmlElement("location")]
    [ForeignKey(nameof(LocationId))]
    [JsonIgnore]
    public Location? Location { get; set; }

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
    [JsonIgnore]
    public int Photos => this.database.Photos.Count(p => p.SlotId == this.SlotId);

    [XmlIgnore]
    public int CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    [JsonIgnore]
    public User? Creator { get; set; }

    [XmlIgnore]
    [NotMapped]
    [JsonIgnore]
    public int PhotosWithAuthor => this.database.Photos.Count(p => p.SlotId == this.SlotId && p.CreatorId == this.CreatorId);

    [XmlIgnore]
    [NotMapped]
    public int Plays => this.PlaysLBP1 + this.PlaysLBP2 + this.PlaysLBP3;

    [XmlIgnore]
    [NotMapped]
    public int PlaysUnique => this.PlaysLBP1Unique + this.PlaysLBP2Unique + this.PlaysLBP3Unique;

    [XmlIgnore]
    [NotMapped]
    public int PlaysComplete => this.PlaysLBP1Complete + this.PlaysLBP2Complete + this.PlaysLBP3Complete;

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
            return ratedLevels.Any() ? ratedLevels.Average(r => r.RatingLBP1) : 3.0;
        }
    }

    [NotMapped]
    [JsonIgnore]
    [XmlElement("reviewCount")]
    public int ReviewCount => this.database.Reviews.Count(r => r.SlotId == this.SlotId);

    [XmlElement("leveltype")]
    public string LevelType { get; set; } = "";
    
    [JsonIgnore]
    public bool Hidden { get; set; }

    [JsonIgnore]
    public string HiddenReason { get; set; } = "";

    // should not be adjustable by user
    public bool CommentsEnabled { get; set; } = true;

    public string Serialize
    (
        GameVersion gameVersion = GameVersion.LittleBigPlanet1,
        RatedLevel? yourRatingStats = null,
        VisitedLevel? yourVisitedStats = null,
        Review? yourReview = null,
        bool fullSerialization = false
    )
    {
        Console.WriteLine(this.ToString());

        int playerCount = RoomHelper.Rooms.Count(r => r.Slot.SlotType == SlotType.User && r.Slot.SlotId == this.SlotId);
        
        string slotData = LbpSerializer.StringElement("id", this.SlotId) +
                          LbpSerializer.StringElement("location", this.Location?.Serialize()) +
                          LbpSerializer.StringElement("npHandle", this.Creator?.Username) +
                          LbpSerializer.StringElement("game", (int)this.GameVersion) +
                          LbpSerializer.StringElement("name", this.Name) +
                          LbpSerializer.StringElement("description", this.Description) +
                          LbpSerializer.StringElement("rootLevel", this.RootLevel) +
                        //   LbpSerializer.StringElement("isAdventurePlanet", this.isAdventurePlanet) +
                          LbpSerializer.StringElement("leveltype", this.LevelType) +
                          LbpSerializer.StringElement("minPlayers", this.MinimumPlayers) +
                          LbpSerializer.StringElement("maxPlayers", this.MaximumPlayers) +
                          LbpSerializer.StringElement("heartCount", this.Hearts) +
                          LbpSerializer.StringElement("thumbsup", this.Thumbsup) +
                          LbpSerializer.StringElement("thumbsdown", this.Thumbsdown) +
                          LbpSerializer.StringElement("averageRating", this.RatingLBP1) +
                          LbpSerializer.StringElement("playerCount", playerCount) +
                          LbpSerializer.StringElement("mmpick", this.TeamPick) +
                          (fullSerialization ? LbpSerializer.StringElement("moveRequired", this.MoveRequired) : "") +
                          (yourRatingStats != null ?
                              LbpSerializer.StringElement<double>("yourRating", yourRatingStats.RatingLBP1, true) +
                              LbpSerializer.StringElement<int>("yourDPadRating", yourRatingStats.Rating, true)
                              : "") +
                          (yourVisitedStats != null ?
                              LbpSerializer.StringElement("yourlbp1PlayCount", yourVisitedStats.PlaysLBP1) +
                              LbpSerializer.StringElement("yourlbp2PlayCount", yourVisitedStats.PlaysLBP2) +
                              LbpSerializer.StringElement("yourlbp3PlayCount", yourVisitedStats.PlaysLBP3)
                              : "") +
                          LbpSerializer.StringElement("reviewCount", this.ReviewCount) +
                          LbpSerializer.StringElement("commentCount", this.Comments) +
                          LbpSerializer.StringElement("photoCount", this.Photos) +
                          LbpSerializer.StringElement("authorPhotoCount", this.PhotosWithAuthor) +
                          LbpSerializer.StringElement("firstPublished", this.FirstUploaded) +
                          LbpSerializer.StringElement("lastUpdated", this.LastUpdated) +
                          (fullSerialization ?
                              yourReview?.Serialize() +
                              LbpSerializer.StringElement("reviewsEnabled", ServerConfiguration.Instance.UserGeneratedContentLimits.LevelReviewsEnabled) +
                              LbpSerializer.StringElement("commentsEnabled", ServerConfiguration.Instance.UserGeneratedContentLimits.LevelCommentsEnabled && this.CommentsEnabled)
                              : "") +
                          LbpSerializer.StringElement("playCount", this.Plays) +
                          LbpSerializer.StringElement("completionCount", this.PlaysComplete) +
                          LbpSerializer.StringElement("lbp1PlayCount", this.PlaysLBP1) +
                          LbpSerializer.StringElement("lbp1CompletionCount", this.PlaysLBP1Complete) +
                          LbpSerializer.StringElement("lbp1UniquePlayCount", this.PlaysLBP1Unique) +
                          LbpSerializer.StringElement("lbp2PlayCount", this.PlaysLBP2) +
                          LbpSerializer.StringElement("lbp2CompletionCount", this.PlaysLBP2Complete) +
                          LbpSerializer.StringElement("uniquePlayCount", this.PlaysLBP2Unique) + // ??? good naming scheme lol

                          LbpSerializer.StringElement("lbp3PlayCount", this.PlaysLBP3) +
                          LbpSerializer.StringElement("lbp3CompletionCount", this.PlaysLBP3Complete) +
                          LbpSerializer.StringElement("lbp3UniquePlayCount", this.PlaysLBP3Unique);

        Console.WriteLine(slotData);
        return LbpSerializer.TaggedStringElement("slot", slotData, "type", "user");
    }
}