using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

public enum CommentType
{
    Profile = 0,
    Level = 1,
}

public class CommentEntity
{
    [Key]
    public int CommentId { get; set; }

    public int PosterUserId { get; set; }

    [ForeignKey(nameof(PosterUserId))]
    public UserEntity Poster { get; set; }

    public CommentType Type { get; set; }

    #nullable enable
    public int? TargetSlotId { get; set; }

    [ForeignKey(nameof(TargetSlotId))]
    public SlotEntity? TargetSlot { get; set; }

    public int? TargetUserId { get; set; }

    [ForeignKey(nameof(TargetUserId))]
    public UserEntity? TargetUser { get; set; }
    #nullable disable

    public bool Deleted { get; set; }

    public string DeletedType { get; set; }

    public string DeletedBy { get; set; }

    public long Timestamp { get; set; }

    public string Message { get; set; }

    public int ThumbsUp { get; set; }
    public int ThumbsDown { get; set; }

    public string GetCommentMessage(DatabaseContext database)
    {
        if (!this.Deleted) return this.Message;

        if (this.DeletedBy == this.Poster.Username) return "This comment has been deleted by the author.";

        UserEntity deletedBy = database.Users.FirstOrDefault(u => u.Username == this.DeletedBy);

        if (deletedBy == null) return "This comment has been deleted.";

        // If the owner of the comment section deletes
        if (deletedBy.UserId == this.TargetUserId || deletedBy.UserId == database.Slots.Find(this.TargetSlotId)?.CreatorId)
            return "This comment has been deleted by the player.";

        if (this.DeletedType == "moderator" && deletedBy.IsModerator)
            return "This comment has been deleted by a moderator.";

        return "This comment has been deleted.";
    }
}