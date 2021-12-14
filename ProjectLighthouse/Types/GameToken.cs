using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Types
{
    public class GameToken
    {
        // ReSharper disable once UnusedMember.Global
        [Key]
        public int TokenId { get; set; }

        public int UserId { get; set; }

        public string UserToken { get; set; }

        public string UserLocation { get; set; }

        public GameVersion GameVersion { get; set; }

        public bool Approved { get; set; } = false;
    }
}