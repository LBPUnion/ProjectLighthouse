using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.PlayerData.Profiles.Email;

public class EmailSetToken
{
    [Key]
    public int EmailSetTokenId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public string EmailToken { get; set; }

    public DateTime ExpiresAt { get; set; }
}