using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    [XmlRoot("comment"), XmlType("comment")]
    public class Comment {
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

        private string serialize() {
            return LbpSerializer.StringElement("id", CommentId) +
                   LbpSerializer.StringElement("npHandle", this.Poster.Username) +
                   LbpSerializer.StringElement("timestamp", Timestamp) +
                   LbpSerializer.StringElement("message", Message) +
                   LbpSerializer.StringElement("thumbsup", ThumbsUp) +
                   LbpSerializer.StringElement("thumbsdown", ThumbsDown);
        }

        public string Serialize(int yourThumb) {
            return LbpSerializer.StringElement("comment", this.serialize() + LbpSerializer.StringElement("yourthumb", yourThumb));
        }

        public string Serialize() {
            return LbpSerializer.StringElement("comment", this.serialize());
        }
    }
}