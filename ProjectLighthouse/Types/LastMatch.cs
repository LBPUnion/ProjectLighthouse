using System.ComponentModel.DataAnnotations;

namespace ProjectLighthouse.Types {
    public class LastMatch {
        [Key] public int UserId { get; set; }
        public long Timestamp { get; set; }
    }
}