using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Profiles;

[XmlRoot("comment")]
[XmlType("comment")]
public class Comment
{
    [Key]
    [XmlAttribute("id")]
    public int CommentId { get; set; }

    public int PosterUserId { get; set; }

    public int TargetId { get; set; }

    [ForeignKey(nameof(PosterUserId))]
    public User Poster { get; set; }

    public bool Deleted { get; set; }

    public string DeletedType { get; set; }

    public string DeletedBy { get; set; }

    public long Timestamp { get; set; }

    [XmlElement("message")]
    public string Message { get; set; }

    public CommentType Type { get; set; }

    public SlotType SlotType { get; set; }

    public int ThumbsUp { get; set; }
    public int ThumbsDown { get; set; }

    [NotMapped]
    [XmlIgnore]
    public int YourThumb;

    public string getComment()
    {
        if (!this.Deleted)
        {
            return this.Message;
        }

        if (this.DeletedBy == this.Poster.Username)
        {
            return "This comment has been deleted by the author.";
        }

        using Database database = new();
        User deletedBy = database.Users.FirstOrDefault(u => u.Username == this.DeletedBy);

        if (deletedBy != null && deletedBy.UserId == this.TargetId)
        {
            return "This comment has been deleted by the player.";
        }

        return "This comment has been deleted.";
    }

    private string serialize()
        => LbpSerializer.StringElement("id", this.CommentId) +
           LbpSerializer.StringElement("npHandle", this.Poster.Username) +
           LbpSerializer.StringElement("timestamp", this.Timestamp) +
           LbpSerializer.StringElement("message", this.Message) +
           (this.Deleted ? LbpSerializer.StringElement("deleted", true) +
             LbpSerializer.StringElement("deletedBy", this.DeletedBy) +
             LbpSerializer.StringElement("deletedType", this.DeletedBy) : "") +
           LbpSerializer.StringElement("thumbsup", this.ThumbsUp) +
           LbpSerializer.StringElement("thumbsdown", this.ThumbsDown);

    public string Serialize(int yourThumb) => LbpSerializer.StringElement("comment", this.serialize() + LbpSerializer.StringElement("yourthumb", yourThumb));

    public string Serialize() => LbpSerializer.StringElement("comment", this.serialize());
}