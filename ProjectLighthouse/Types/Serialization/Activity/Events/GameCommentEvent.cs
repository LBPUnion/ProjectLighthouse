using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Serialization.Review;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

[XmlInclude(typeof(GameUserCommentEvent))]
[XmlInclude(typeof(GameSlotCommentEvent))]
public class GameCommentEvent : GameEvent
{
    [XmlElement("comment_id")]
    public int CommentId { get; set; }
}

public class GameUserCommentEvent : GameCommentEvent
{
    [XmlElement("object_user")]
    public string TargetUsername { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        CommentEntity comment = await database.Comments.FindAsync(this.CommentId);
        if (comment == null) return;

        UserEntity user = await database.Users.FindAsync(comment.TargetUserId);
        if (user == null) return;

        this.TargetUsername = user.Username;
    }
}

public class GameSlotCommentEvent : GameCommentEvent
{
    [XmlElement("object_slot_id")]
    public ReviewSlot TargetSlot { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);

        CommentEntity comment = await database.Comments.FindAsync(this.CommentId);
        if (comment == null) return;

        SlotEntity slot = await database.Slots.FindAsync(comment.TargetSlotId);

        if (slot == null) return;

        this.TargetSlot = ReviewSlot.CreateFromEntity(slot);
    }
}