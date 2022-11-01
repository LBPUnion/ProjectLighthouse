using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.EntityFrameworkCore;

#nullable enable

public class Activity
{
    [Key]
    public int ActivityId { get; set; }
    /*
        Where category is equal to LEVEL and target id is equal to 1 get SLOT OBJECT
        Where category is equal to USER and target id is equal to 1 get CREATOR OBJECT
        Event
    */
    public ActivityType ActivityType { get; set; }

    public int ActivityTargetId { get; set; }

    public string ExtrasCollection { get; set; } = "";

    [NotMapped]
    public int[] Extras
    {
        get
        {
            string[] userIds = ExtrasCollection.Split(",");
            if (userIds[0] == "") return new int[0];
            return Array.ConvertAll(userIds, u => {
                int parsed = 0;
                int.TryParse(u, out parsed);
                return parsed;
            });
        }
        set => ExtrasCollection = string.Join(",", value);
    }
}