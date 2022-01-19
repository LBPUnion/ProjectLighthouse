using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Profiles;

[XmlRoot("comment")]
[XmlType("comment")]
public class Comment
{
    [Key]
    [XmlAttribute("id")]
    public int CommentId { get; set; }

    public int PosterUserId { get; set; }

    public int TargetUserId { get; set; }

    [ForeignKey(nameof(PosterUserId))]
    public User Poster { get; set; }

    [ForeignKey(nameof(TargetUserId))]
    public User Target { get; set; }

    public long Timestamp { get; set; }

    [XmlElement("message")]
    public string Message { get; set; }

    public int ThumbsUp { get; set; }
    public int ThumbsDown { get; set; }

    private string serialize()
        => LbpSerializer.StringElement("id", this.CommentId) +
           LbpSerializer.StringElement("npHandle", this.Poster.Username) +
           LbpSerializer.StringElement("timestamp", this.Timestamp) +
           LbpSerializer.StringElement("message", this.Message) +
           LbpSerializer.StringElement("thumbsup", this.ThumbsUp) +
           LbpSerializer.StringElement("thumbsdown", this.ThumbsDown);

    public string Serialize(int yourThumb) => LbpSerializer.StringElement("comment", this.serialize() + LbpSerializer.StringElement("yourthumb", yourThumb));

    public string Serialize() => LbpSerializer.StringElement("comment", this.serialize());
}