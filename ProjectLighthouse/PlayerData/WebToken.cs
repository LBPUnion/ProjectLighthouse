using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.PlayerData;

public class WebToken
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string UserToken { get; set; }
}