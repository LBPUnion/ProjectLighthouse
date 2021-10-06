using System.ComponentModel.DataAnnotations;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    public class Comment {
        [Key] public int CommentId { get; set; }

        public string Username { get; set; }
        public long Timestamp { get; set; }
        public string Message { get; set; }
        public int ThumbsUp { get; set; }
        public int ThumbsDown { get; set; }

        private string serialize() {
            return LbpSerializer.StringElement("id", CommentId) +
                   LbpSerializer.StringElement("npHandle", Username) +
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