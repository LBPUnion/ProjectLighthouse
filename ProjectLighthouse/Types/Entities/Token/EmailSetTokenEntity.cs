using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Token;

public class EmailSetTokenEntity
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int EmailSetTokenId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public string EmailToken { get; set; }

    public DateTime ExpiresAt { get; set; }
}