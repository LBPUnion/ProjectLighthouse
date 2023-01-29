using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Entities.Profile;
using LBPUnion.ProjectLighthouse.Users;

namespace LBPUnion.ProjectLighthouse.Entities.Token;

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

    public string TicketHash { get; set; }

    public DateTime ExpiresAt { get; set; }
}