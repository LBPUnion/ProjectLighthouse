using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types;

public class AuthenticationAttempt
{
    [Key]
    public int AuthenticationAttemptId { get; set; }

    public long Timestamp { get; set; }
    public Platform Platform { get; set; }
    public string IPAddress { get; set; }

    public int GameTokenId { get; set; }

    [ForeignKey(nameof(GameTokenId))]
    public GameToken GameToken { get; set; }
}