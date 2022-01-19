using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Types;

public class WebToken
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string UserToken { get; set; }
}