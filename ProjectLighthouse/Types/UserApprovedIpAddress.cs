using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types
{
    public class UserApprovedIpAddress
    {
        [Key]
        public int UserApprovedIpAddressId { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public string IpAddress { get; set; }
    }
}