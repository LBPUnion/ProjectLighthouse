using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types;

public class GameToken
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int TokenId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public string UserToken { get; set; }

    public string UserLocation { get; set; }

    public GameVersion GameVersion { get; set; }

    public Platform Platform { get; set; }

    // Set by /authentication webpage
    public bool Approved { get; set; }

    // Set to true on login
    public bool Used { get; set; }
}