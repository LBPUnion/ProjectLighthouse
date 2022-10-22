using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
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

    public int Category { get; set; }

    public long Timestamp { get; set; }

    public int DestinationId { get; set; }

    public string ActionCollection { get; set; } = "";

    [NotMapped]
    public int[] Actions
    {
        get
        {
            string[] actionIds = ActionCollection.Split(",");
            return Array.ConvertAll(actionIds, a => int.Parse(a));
        }
        set => ActionCollection = string.Join(",", value);
    }

    public string ActorCollection { get; set; } = "";

    [NotMapped]
    public int[] Actors
    {
        get
        {
            string[] actorIds = ActorCollection.Split(",");
            return Array.ConvertAll(actorIds, a => int.Parse(a));
        }
        set => ActorCollection = string.Join(",", value);
    }

    public async Task<string> SerializeAsync(GameVersion gameVersion = GameVersion.LittleBigPlanet3)
    {
        string groupData = "";

        List<ACTActionCollection> groups = new List<ACTActionCollection>();
        foreach (var actionId in Actions)
        {
            ACTActionCollection? actionStaging = await this.database.ACTActionCollection.Include(g => g.Actor).FirstOrDefaultAsync(g => g.ActionId == actionId);
            if (actionStaging == null) break;
            groups.Add(actionStaging);
        }

        string data = "";
        data += LbpSerializer.StringElement("timestamp", Timestamp);
        switch ((ActivityCategory)Category)
        {
            case ActivityCategory.News:
                break;
            case ActivityCategory.TeamPick:
                break;
            case ActivityCategory.Level:
                data += LbpSerializer.TaggedStringElement("slot_id", DestinationId, "type", "user");
                data += await SerializeSubgroups(groups);
                groupData += LbpSerializer.TaggedStringElement("group", data, "type", "level");
                break;
            case ActivityCategory.User:
                User? user = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == DestinationId);
                data += LbpSerializer.StringElement("user_id", user?.Username);
                data += await SerializeSubgroups(groups);
                groupData += LbpSerializer.TaggedStringElement("group", data, "type", "user");
                break;
        }

        return groupData;
    }

    private async Task<string> SerializeSubgroups(IEnumerable<ACTActionCollection> groups)
    {
        string subGroups = "";
        string groupData = "";
        string eventData = "";

        foreach (ACTActionCollection group in groups)
        {
            eventData += await SerializeEvents(group);
        }
        groupData += LbpSerializer.StringElement("timestamp", groups.First().ActionTimestamp) +
                     LbpSerializer.StringElement("user_id", groups.First().Actor?.Username) +
                     LbpSerializer.StringElement("events", eventData);

        subGroups += LbpSerializer.TaggedStringElement("group", groupData, "type", "user");

        return LbpSerializer.StringElement("subgroups", subGroups);
    }

    private async Task<string> SerializeEvents(ACTActionCollection group)
    {
        string objectType = ActivityHelper.ObjectType(group.ActionType);
        string eventData = LbpSerializer.StringElement("timestamp", group.ActionTimestamp) +
                           LbpSerializer.StringElement("actor", group.Actor?.Username);
        if (objectType == "object_slot_id")
        {
            eventData += LbpSerializer.TaggedStringElement(objectType, group.ObjectId, "type", "user");
        }
        else if (objectType == "object_user")
        {
            User? objectUser = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == group.ObjectId);
            eventData += LbpSerializer.StringElement(objectType, objectUser?.Username);
        }
        else
        {
            eventData += LbpSerializer.StringElement(objectType, group.ObjectId);
        }
        eventData += FormInteraction(group);


        return LbpSerializer.TaggedStringElement("event", eventData, "type", ActivityHelper.EventTypeAsString(group.ActionType));
    }

    private string FormInteraction(ACTActionCollection group)
    {
        switch ((EventType)group.ActionType)
        {
            case EventType.DpadRating:
                return LbpSerializer.StringElement("dpad_rating", group.Interaction);
            case EventType.LBP1Rate:
                return LbpSerializer.StringElement("rating", group.Interaction);
            case EventType.Score:
                return LbpSerializer.StringElement("score", group.Interaction) +
                       LbpSerializer.StringElement("user_count", group.Interaction2);
            case EventType.CommentUser:
            case EventType.CommentLevel:
                return LbpSerializer.StringElement("comment_id", group.Interaction);
            case EventType.UploadPhoto:
                return LbpSerializer.StringElement("photo_id", group.Interaction);
            case EventType.PlayLevel:
                return LbpSerializer.StringElement("count", group.Interaction);
            case EventType.PublishLevel:
                return LbpSerializer.StringElement("republish", group.Interaction) +
                       LbpSerializer.StringElement("count", group.Interaction2);
            case EventType.Review:
                return LbpSerializer.StringElement("review_id", group.Interaction) +
                       LbpSerializer.StringElement("review_modified", group.Interaction2);
            default: return "";
        }
    }
}