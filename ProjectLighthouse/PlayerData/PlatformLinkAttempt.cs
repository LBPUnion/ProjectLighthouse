﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;

namespace LBPUnion.ProjectLighthouse.PlayerData;

public class PlatformLinkAttempt
{
    [Key]
    public int PlatformLinkAttemptId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int UserId { get; set; }

    public ulong PlatformId { get; set; }

    public Platform Platform { get; set; }

    public long Timestamp { get; set; }

    public string IPAddress { get; set; } = "";

}