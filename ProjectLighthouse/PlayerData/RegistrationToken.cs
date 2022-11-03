using System;
using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.PlayerData
{
    public class RegistrationToken
    {
        [Key]
        public int TokenId { get; set; }

        public string Token { get; set; }

        public DateTime Created { get; set; }

        public string Username { get; set; }
    }
}

