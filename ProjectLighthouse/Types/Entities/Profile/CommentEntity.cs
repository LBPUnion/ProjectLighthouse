using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LBPUnion.ProjectLighthouse.Database;

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

    public int TargetId { get; set; }

    [ForeignKey(nameof(PosterUserId))]
    public UserEntity Poster { get; set; }

    public bool Deleted { get; set; }

    public string DeletedType { get; set; }

    public string DeletedBy { get; set; }

    public long Timestamp { get; set; }

    public string Message { get; set; }

    public CommentType Type { get; set; }

    public int ThumbsUp { get; set; }
    public int ThumbsDown { get; set; }

    public string GetCommentMessage()
    {
        if (!this.Deleted)
        {
            return this.Message;
        }

        if (this.DeletedBy == this.Poster.Username)
        {
            return "This comment has been deleted by the author.";
        }

        using DatabaseContext database = DatabaseContext.CreateNewInstance();
        UserEntity deletedBy = database.Users.FirstOrDefault(u => u.Username == this.DeletedBy);

        if (deletedBy != null && deletedBy.UserId == this.TargetId)
        {
            return "This comment has been deleted by the player.";
        }

        return "This comment has been deleted.";
    }
}