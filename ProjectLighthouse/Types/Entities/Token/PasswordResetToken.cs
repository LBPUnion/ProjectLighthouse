using System;
using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Entities.Token;

public class PasswordResetToken
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string ResetToken { get; set; }

    public DateTime Created { get; set; }
}