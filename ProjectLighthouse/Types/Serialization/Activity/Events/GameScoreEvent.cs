using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GameScoreEvent : GameEvent
{
    [XmlIgnore]
    public int ScoreId { get; set; }

    [XmlElement("object_slot_id")]
    public ReviewSlot Slot { get; set; }

    [XmlElement("score")]
    public int Score { get; set; }

    [DefaultValue(0)]
    [XmlElement("user_count")]
    public int UserCount { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        ScoreEntity score = await database.Scores.FindAsync(this.ScoreId);
        if (score == null) return;

        SlotEntity slot = await database.Slots.FindAsync(score.SlotId);
        if (slot == null) return;

        this.Score = score.Points;
        //TODO is this correct?
        this.UserCount = score.Type == 7 ? 0 : score.Type;

        this.Slot = ReviewSlot.CreateFromEntity(slot);
    }
}