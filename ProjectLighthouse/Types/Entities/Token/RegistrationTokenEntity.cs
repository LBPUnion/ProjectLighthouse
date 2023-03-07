using System;
using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Token;

public class RegistrationTokenEntity
{
    // ReSharper disable once UnusedMember.Global
    [Key]
    public int TokenId { get; set; }

    public string Token { get; set; }

    public DateTime Created { get; set; }

    public string Username { get; set; }
}