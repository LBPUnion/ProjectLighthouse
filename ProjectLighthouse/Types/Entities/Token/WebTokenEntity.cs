using System;
using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Token;

public class WebTokenEntity
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string UserToken { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool Verified { get; set; }
}