﻿
using System;
using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.PlayerData
{
    public class APIKey
    {
        [Key]
        public int Id { get; set; }

        public string Description { get; set; }

        public string Key { get; set; }

        public DateTime Created { get; set; }
    }
}

