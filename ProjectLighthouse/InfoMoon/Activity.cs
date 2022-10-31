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
    [NotMapped]
    private Database? _database;

    [NotMapped]
    private Database database
    {
        get
        {
            if (this._database != null) return this._database;

            return this._database = new Database();
        }
        set => this._database = value;
    }

    [Key]
    public int ActivityId { get; set; }
    /*
        Where category is equal to LEVEL and target id is equal to 1 get SLOT OBJECT
        Where category is equal to USER and target id is equal to 1 get CREATOR OBJECT
        Event
    */
    public int TargetType { get; set; }

    [NotMapped]
    public ActivityType Category
    {
        get => (ActivityType)TargetType;
        set => TargetType = (int)value;
    }

    public int TargetId { get; set; }

    public string UserCollection { get; set; } = "";

    [NotMapped]
    public int[] Users
    {
        get
        {
            string[] userIds = UserCollection.Split(",");
            if (userIds[0] == "") return new int[0];
            return Array.ConvertAll(userIds, u => {
                int parsed = 0;
                int.TryParse(u, out parsed);
                return parsed;
            });
        }
        set => UserCollection = string.Join(",", value);
    }
}