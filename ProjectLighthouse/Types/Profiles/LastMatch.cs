using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Types.Profiles
{
    public class LastMatch
    {
        [Key]
        public int UserId { get; set; }

        public long Timestamp { get; set; }
    }
}