using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("deleted_by")]
public enum DeletedBy
{
    [XmlEnum(Name = "none")]
    None,
    [XmlEnum(Name = "moderator")]
    Moderator,
    [XmlEnum(Name = "level_author")]
    LevelAuthor,
}

[XmlRoot("review")]
[XmlType("review")]
public class GameReview : ILbpSerializable, INeedsPreparationForSerialization
{
    [XmlIgnore]
    public int TargetUserId { get; set; }

    [XmlIgnore]
    public int ReviewerId { get; set; }

    [XmlIgnore]
    public int ReviewId { get; set; }

    [XmlAttribute("id")]
    public string ReviewTag { get; set; }

    [XmlElement("slot_id")]
    public ReviewSlot Slot { get; set; }

    [XmlElement("reviewer")]
    public string ReviewerUsername { get; set; }

    [XmlElement("timestamp")]
    public long Timestamp { get; set; }

    [XmlElement("labels")]
    public string LabelCollection { get; set; } = "";

    [DefaultValue(false)]
    [XmlElement("deleted")]
    public bool Deleted { get; set; }

    [DefaultValue(DeletedBy.None)]
    [XmlElement("deleted_by")]
    public DeletedBy DeletedBy { get; set; }

    [XmlElement("text")]
    public string Text { get; set; } = "";

    [XmlElement("thumb")]
    public int Thumb { get; set; }

    [XmlElement("thumbsup")]
    public int ThumbsUp { get; set; }

    [XmlElement("thumbsdown")]
    public int ThumbsDown { get; set; }

    [XmlElement("yourthumb")]
    public int YourThumb { get; set; }

    public async Task PrepareSerialization(DatabaseContext database)
    {
        this.YourThumb = await database.RatedReviews.Where(r => r.UserId == this.TargetUserId && r.ReviewId == this.ReviewId)
            .Select(r => r.Thumb)
            .FirstOrDefaultAsync();

        this.ReviewerUsername = await database.Users.Where(u => u.UserId == this.ReviewerId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync();

        // Slot could be a developer level so check here
        this.Slot.SlotType = await database.Slots.Where(s => s.SlotId == this.Slot.SlotId)
            .Select(s => s.Type)
            .FirstOrDefaultAsync();

        this.ReviewTag = $"{this.Slot.SlotId}.{this.ReviewerUsername}";
    }

    public static GameReview CreateFromEntity(ReviewEntity entity, GameTokenEntity token) 
        => CreateFromEntity(entity, token.UserId);

    public static GameReview CreateFromEntity(ReviewEntity entity, int targetUserId) =>
        new()
        {
            ReviewerId = entity.ReviewerId,
            ReviewId = entity.ReviewId,
            Slot = new ReviewSlot
            {
                SlotId = entity.SlotId,
                SlotType = SlotType.User,
            },
            Text = entity.Text,
            Deleted = entity.Deleted,
            DeletedBy = entity.DeletedBy,
            LabelCollection = entity.LabelCollection,
            Thumb = entity.Thumb,
            ThumbsUp = entity.ThumbsUp,
            ThumbsDown = entity.ThumbsDown,
            Timestamp = entity.Timestamp,
            TargetUserId = targetUserId,
        };

}