#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("slot")]
public class GameUserSlot : SlotBase, INeedsPreparationForSerialization
{
    [XmlIgnore]
    public int CreatorId { get; set; }

    [XmlIgnore]
    public GameVersion TargetGame { get; set; }

    [XmlIgnore]
    public int TargetUserId { get; set; }

    [XmlIgnore]
    public SerializationMode SerializationMode { get; set; }

    [XmlElement("id")]
    public int SlotId { get; set; }

    [XmlAttribute("type")]
    public string Type { get; set; } = "user";

    [XmlElement("npHandle")]
    public NpHandle AuthorHandle { get; set; } = new();

    [XmlElement("location")]
    public Location Location { get; set; } = new();

    [XmlElement("game")]
    public GameVersion GameVersion { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = "";

    [XmlElement("description")]
    public string Description { get; set; } = "";

    [XmlElement("rootLevel")]
    public string? RootLevel { get; set; }

    [XmlElement("resource")]
    public string[]? Resources { get; set; }
    public bool ShouldSerializeResources() => false;

    [XmlElement("icon")]
    public string IconHash { get; set; } = "";

    [XmlElement("initiallyLocked")]
    public bool InitiallyLocked { get; set; }

    [XmlElement("isSubLevel")]
    public bool IsSubLevel { get; set; }

    [XmlElement("isLBP1Only")]
    public bool IsLbp1Only { get; set; }

    [XmlElement("isAdventurePlanet")]
    public bool IsAdventurePlanet { get; set; }

    [DefaultValue("")]
    [XmlElement("background")]
    public string BackgroundHash { get; set; } = "";

    [XmlElement("shareable")]
    public int IsShareable { get; set; }

    [XmlElement("authorLabels")]
    public string AuthorLabels { get; set; } = "";

    [XmlElement("leveltype")]
    public string LevelType { get; set; } = "";

    [XmlElement("minPlayers")]
    public int MinimumPlayers { get; set; }

    [XmlElement("maxPlayers")]
    public int MaximumPlayers { get; set; }

    [XmlElement("heartCount")]
    public int HeartCount { get; set; }

    [XmlElement("thumbsup")]
    public int ThumbsUp { get; set; }

    [XmlElement("thumbsdown")]
    public int ThumbsDown { get; set; }

    [XmlElement("averageRating")]
    public double AverageRating { get; set; }
    public bool ShouldSerializeAverageRating() => this.GameVersion == GameVersion.LittleBigPlanet1;

    [XmlElement("playerCount")]
    public int PlayerCount { get; set; }

    [XmlElement("mmpick")]
    public bool IsTeamPicked { get; set; }

    [XmlElement("moveRequired")]
    public bool IsMoveRequired { get; set; }
    public bool ShouldSerializeIsMoveRequired() => this.SerializationMode == SerializationMode.Full;

    [XmlElement("vitaCrossControlRequired")]
    public bool IsCrossControlRequired { get; set; }
    public bool ShouldSerializeIsCrossControlRequired() => this.SerializationMode == SerializationMode.Full;

    [DefaultValue(0.0)]
    [XmlElement("yourRating")]
    public double YourRating { get; set; }

    [DefaultValue(0)]
    [XmlElement("yourDPadRating")]
    public double YourDPadRating { get; set; }

    [XmlElement("yourlbp1PlayCount")]
    public int YourPlaysLBP1 { get; set; }

    [XmlElement("yourlbp2PlayCount")]
    public int YourPlaysLBP2 { get; set; }

    [XmlElement("yourlbp3PlayCount")]
    public int YourPlaysLBP3 { get; set; }

    [XmlElement("reviewCount")]
    public int ReviewCount { get; set; }

    [XmlElement("commentCount")]
    public int CommentCount { get; set; }

    [XmlElement("photoCount")]
    public int PhotoCount { get; set; }

    [XmlElement("authorPhotoCount")]
    public int AuthorPhotoCount { get; set; }

    [DefaultValue("")]
    [XmlElement("tags")]
    public string? Tags { get; set; }
    public bool ShouldSerializeTags() => this.SerializationMode == SerializationMode.Full;

    [DefaultValue("")]
    [XmlElement("labels")]
    // The C# XML serializer doesn't serialize fields that don't have public getters and setters
    // even though it doesn't use the setter, these fields were originally meant to be expression bodies to another variable
    // but unfortunately that's not supported.
    public string Labels {
        get => this.AuthorLabels;
        set => throw new NotSupportedException();
    }
    public bool ShouldSerializeLabels() => this.SerializationMode == SerializationMode.Full;

    [XmlElement("firstPublished")]
    public long FirstUploaded { get; set; }

    [XmlElement("lastUpdated")]
    public long LastUpdated { get; set; }

    [DefaultValue(null)]
    [XmlElement("yourReview")]
    public GameReview? YourReview { get; set; }
    public bool ShouldSerializeYourReview() => this.SerializationMode == SerializationMode.Full; 

    [XmlElement("reviewsEnabled")]
    public bool ReviewsEnabled
    {
        get => ServerConfiguration.Instance.UserGeneratedContentLimits.LevelReviewsEnabled;
        set => throw new NotSupportedException();
    }
    public bool ShouldSerializeReviewsEnabled() => this.SerializationMode == SerializationMode.Full;

    [XmlElement("commentsEnabled")]
    public bool CommentsEnabled
    {
        get => ServerConfiguration.Instance.UserGeneratedContentLimits.LevelCommentsEnabled;
        set => throw new NotSupportedException();
    }
    public bool ShouldSerializeCommentsEnabled() => this.SerializationMode == SerializationMode.Full;

    [XmlElement("playCount")]
    public int PlayCount { get; set; }

    [XmlElement("completionCount")]
    public int CompletePlayCount { get; set; }

    [XmlElement("lbp1PlayCount")]
    public int LBP1PlayCount { get; set; }

    [XmlElement("lbp1CompletePlayCount")]
    public int LBP1CompletePlayCount { get; set; }

    [XmlElement("lbp1UniquePlayCount")]
    public int LBP1UniquePlayCount { get; set; }

    [XmlElement("lbp2PlayCount")]
    public int LBP2PlayCount { get; set; }

    [XmlElement("lbp2CompletePlayCount")]
    public int LBP2CompletePlayCount { get; set; }

    [XmlElement("uniquePlayCount")]
    public int LBP2UniquePlayCount { get; set; }

    [XmlElement("lbp3PlayCount")]
    public int LBP3PlayCount { get; set; }

    [XmlElement("lbp3CompletePlayCount")]
    public int LBP3CompletePlayCount { get; set; }

    [XmlElement("lbp3UniquePlayCount")]
    public int LBP3UniquePlayCount { get; set; }

    [DefaultValue(0)]
    [XmlElement("sizeOfResources")]
    public int ResourcesSize { get; set; }
    public bool ShouldSerializeResourcesSize() => this.TargetGame == GameVersion.LittleBigPlanetVita;

    public async Task PrepareSerialization(DatabaseContext database)
    {
        var stats = await database.Slots
            .Select(_ => new
            {
                ThumbsUp = database.RatedLevels.Count(r => r.SlotId == this.SlotId && r.Rating == 1),
                ThumbsDown = database.RatedLevels.Count(r => r.SlotId == this.SlotId && r.Rating == -1),
                ReviewCount = database.Reviews.Count(r => r.SlotId == this.SlotId),
                CommentCount = database.Comments.Count(c => c.TargetId == this.SlotId && c.Type == CommentType.Level),
                PhotoCount = database.Photos.Count(p => p.SlotId == this.SlotId),
                AuthorPhotoCount = database.Photos.Count(p => p.SlotId == this.SlotId && p.CreatorId == this.CreatorId),
                HeartCount = database.HeartedLevels.Count(h => h.SlotId == this.SlotId),
                Username = database.Users.Where(u => u.UserId == this.CreatorId).Select(u => u.Username).First(),
            })
            .FirstAsync();
        ReflectionHelper.CopyAllFields(stats, this);
        this.AuthorHandle = new NpHandle(stats.Username, "");

        if (this.GameVersion == GameVersion.LittleBigPlanet1)
        {
            this.AverageRating = database.RatedLevels.Where(r => r.SlotId == this.SlotId)
                .Average(r => (double?)r.RatingLBP1) ?? 3.0;
            SortedDictionary<string, int> tagOccurrences = new();
            foreach (RatedLevelEntity r in await database.RatedLevels
                         .Where(r => r.SlotId == this.SlotId && r.TagLBP1.Length > 0)
                         .ToListAsync())
            {
                if (tagOccurrences.TryGetValue(r.TagLBP1, out _))
                    tagOccurrences[r.TagLBP1]++;
                else
                    tagOccurrences.Add(r.TagLBP1, 1);
            }
            this.Tags = string.Join(",", tagOccurrences.OrderBy(r => r.Value).Select(r => r.Key).ToList());
        }

        if (this.SerializationMode == SerializationMode.Minimal) return;

        if (this.GameVersion == GameVersion.LittleBigPlanetVita && this.Resources != null) this.ResourcesSize = this.Resources.Sum(FileHelper.ResourceSize);

        #nullable enable
        RatedLevelEntity? yourRating = await database.RatedLevels.FirstOrDefaultAsync(r => r.UserId == this.TargetUserId && r.SlotId == this.SlotId);
        ReviewEntity? yourReview = await database.Reviews.FirstOrDefaultAsync(r => r.ReviewerId == this.TargetUserId && r.SlotId == this.SlotId);
        VisitedLevelEntity? yourVisitedStats = await database.VisitedLevels.FirstOrDefaultAsync(v => v.UserId == this.TargetUserId && v.SlotId == this.SlotId);
        if (yourRating != null)
        {
            this.YourRating = this.GameVersion == GameVersion.LittleBigPlanet1 ? yourRating.Rating : yourRating.RatingLBP1;
            this.YourDPadRating = yourRating.Rating;
        }
        if (yourVisitedStats != null)
        {
            this.YourPlaysLBP1 = yourVisitedStats.PlaysLBP1;
            this.YourPlaysLBP2 = yourVisitedStats.PlaysLBP2;
            this.YourPlaysLBP3 = yourVisitedStats.PlaysLBP3;
        }
        if (yourReview != null)
        {
            this.YourReview = GameReview.CreateFromEntity(yourReview, this.TargetUserId);
        }
        #nullable disable

        this.PlayerCount = RoomHelper.Rooms.Count(r => r.Slot.SlotType == SlotType.User && r.Slot.SlotId == this.SlotId);
    }

}