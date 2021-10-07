using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        private string posterUsername;

//        [XmlAttribute("username")]
        public string PosterUsername {
            get {
                if(this.posterUsername != null) return this.posterUsername;
                return this.posterUsername = new Database().Users.First(u => u.UserId == PosterUserId).Username;
            }
        }

        private string targetUsername;

        public string TargetUsername {
            get {
                if(this.targetUsername != null) return this.targetUsername;

                return this.targetUsername = new Database().Users.First(u => u.UserId == TargetUserId).Username;
            }
        }

        public long Timestamp { get; set; }

        [XmlElement("message")]
        public string Message { get; set; }
        public int ThumbsUp { get; set; }
        public int ThumbsDown { get; set; }

        private string serialize() {
            return LbpSerializer.StringElement("id", CommentId) +
                   LbpSerializer.StringElement("npHandle", this.PosterUsername) +
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