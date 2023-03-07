using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Token;

public class GameTokenEntity
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int TokenId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public string UserToken { get; set; }

    public string UserLocation { get; set; }

    public GameVersion GameVersion { get; set; }

    public Platform Platform { get; set; }

    public string TicketHash { get; set; }

    public DateTime ExpiresAt { get; set; }
}