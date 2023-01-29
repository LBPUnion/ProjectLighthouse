using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Entities.Token;

public class EmailSetToken
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int EmailSetTokenId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public string EmailToken { get; set; }

    public DateTime ExpiresAt { get; set; }
}