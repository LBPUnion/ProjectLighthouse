using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LBPUnion.ProjectLighthouse.Types;

public class HeartedProfile
{
    // ReSharper disable once UnusedMember.Global
    [Obsolete($"Use {nameof(HeartedUserId)} instead, this is a key which you should never need to use.")]
    [Key]
    public int HeartedProfileId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int HeartedUserId { get; set; }

    [ForeignKey(nameof(HeartedUserId))]
    public User HeartedUser { get; set; }
}