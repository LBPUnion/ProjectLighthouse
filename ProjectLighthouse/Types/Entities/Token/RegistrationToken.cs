using System;
using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Entities.Token;

public class RegistrationToken
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int TokenId { get; set; }

    public string Token { get; set; }

    public DateTime Created { get; set; }

    public string Username { get; set; }
}