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

public class ActivityStream {
    [NotMapped]
    private Database? _database;

    [NotMapped]
    private Database database {
        get {
            if (this._database != null) return this._database;

            return this._database = new Database();
        }
        set => this._database = value;
    }
    
    [Key]
    public int ActivityId { get; set; }

    public long Timestamp { get; set; } 

    public int TargetId { get; set; }

    public int ActorId { get; set; }

    [ForeignKey(nameof(ActorId))]
    public User? Actor { get; set; }

    public string EventTypeCollection { get; set; } = "";
    
    [NotMapped]
    public string[] EventTypes 
    { 
        get => EventTypeCollection.Split(",");
        set => EventTypeCollection = string.Join(",", value);
    }

    public string ObjectCollection { get; set; } = "";

    [NotMapped]
    public string[] Objects 
    {
        get => ObjectCollection.Split(",");
        set => ObjectCollection = string.Join(",", value);
    }

    public string EventTimestampCollection { get; set; } = "";

    [NotMapped]
    public string[] EventTimestamps 
    {
        get => EventTimestampCollection.Split(",");
        set => EventTimestampCollection = string.Join(",", value);
    } 

    public string InteractCollection { get; set; } = "1"; // Unfinished 

    public async Task<string> Serialize(GameVersion gameVersion = GameVersion.LittleBigPlanet3)
    {
        if (gameVersion == GameVersion.LittleBigPlanet1 || gameVersion == GameVersion.LittleBigPlanetPSP) return ""; // Unsupported
        EventType primaryEvent = StreamHelper.convertPost(EventTypes[0]);

        string groupType = "";
        string groupData = "";
        string eventData = "";

        groupData += LbpSerializer.StringElement("timestamp", this.Timestamp);
        switch (primaryEvent)
        {
            case EventType.News:
                // This doesn't do anything or matter for News events but we're going to 
                // use it anyway as LBP expects it for the other types and we want to be neat.
                groupType = "news"; 
                groupData += LbpSerializer.StringElement("news_id", this.TargetId);
                eventData += SerializeGeneric(EventType.News);
                break;
            case EventType.TeamPick:
                if (gameVersion == GameVersion.LittleBigPlanet2 || 
                    gameVersion == GameVersion.LittleBigPlanetVita) return ""; // Unsupported
                groupType = "level";
                groupData += LbpSerializer.TaggedStringElement("slot_id", this.TargetId, "type", "user");
                eventData += SerializeGeneric(EventType.TeamPick);
                break;
            case EventType.HeartUser:
            case EventType.CommentUser:
                User? userTarget = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == this.TargetId);
                if(userTarget == null) return "";
                groupType = "user";
                groupData += LbpSerializer.StringElement("user_id",  userTarget.Username);
                eventData += SerializeSubgroups(EventTypes, EventTimestamps, Objects, userTarget);
                break;
            default:
                groupType = "level";
                groupData += LbpSerializer.TaggedStringElement("slot_id", this.TargetId, "type", "user");
                eventData += SerializeSubgroups(EventTypes, EventTimestamps, Objects);
                break;
        }

        groupData += eventData;

        return LbpSerializer.TaggedStringElement("group", groupData, "type", groupType);
    }

    string SerializeGeneric(EventType activityType)
    {
        string genericEvent = "";
        string genEventData = "";

        genEventData += LbpSerializer.StringElement("timestamp", this.Timestamp); // The timestamp of a single event stream post will always be equal to the event itself

        if (activityType == EventType.News) genEventData += LbpSerializer.StringElement("news_id", this.TargetId);
        else genEventData += LbpSerializer.TaggedStringElement("object_slot_id", this.Objects[0], "type", "user");

        genericEvent += LbpSerializer.TaggedStringElement("event", genEventData, "type", 
            (activityType == EventType.TeamPick) ? "mm_pick_level" : "news_post"
        );

        return LbpSerializer.StringElement("events", genericEvent);
    }

    public string SerializeSubgroups(string[] events, string[] timestamps, string[] objects, User? target = null)
    {
        if (this.Actor == null) return "";
        string groupData = LbpSerializer.StringElement("timestamp", timestamps[0]) +
                           LbpSerializer.StringElement("user_id", this.Actor.Username);
        string eventData = "";
        string groupType = "";

        int index = 0;
        foreach(string interaction in events)
        {
            EventType eventType = StreamHelper.convertPost(interaction);
            string interactionEvent = LbpSerializer.StringElement("timestamp", timestamps[index]) +
                                      LbpSerializer.StringElement("actor", this.Actor.Username);
            switch (eventType)
            {
                case EventType.UploadPhoto:
                case EventType.DpadRating:
                case EventType.Score:
                case EventType.PublishLevel:
                case EventType.HeartLevel:
                case EventType.PlayLevel:
                    interactionEvent += LbpSerializer.TaggedStringElement("object_slot_id", this.TargetId, "type", "user");
                    groupType = "level";
                    break;
                case EventType.HeartUser:
                case EventType.CommentUser:
                    if (target == null) return "";
                    interactionEvent += LbpSerializer.StringElement("object_user", target.Username);
                    groupType = "user";
                    break;
                default:
                    break;
            }
            switch (eventType)
            {
                case EventType.UploadPhoto:
                    interactionEvent += LbpSerializer.StringElement("photo_id", objects[index]);
                    break;
                case EventType.DpadRating:
                    interactionEvent += LbpSerializer.StringElement("dpad_rating", objects[index]);
                    break;
                case EventType.Score:
                    interactionEvent += LbpSerializer.StringElement("score", objects[index]) +
                                        LbpSerializer.StringElement("user_count", 1); // TODO: use score id, get score from database. For now this will be static 1 players.
                    break;
                case EventType.HeartLevel:
                case EventType.PlayLevel:
                case EventType.HeartUser:
                    interactionEvent += LbpSerializer.StringElement("count", 1); // TODO: Count how many times this level was replayed by the actor in a 30 minute timespan.
                    break;
                case EventType.CommentUser:
                    interactionEvent += LbpSerializer.StringElement("comment_id", objects[index]);
                    break;
                case EventType.PublishLevel:
                default:
                    break;
            }

            eventData += LbpSerializer.TaggedStringElement("event", interactionEvent, "type", interaction);
            index++;
        }

        groupData += LbpSerializer.StringElement("events", eventData);
        return LbpSerializer.StringElement("subgroups", LbpSerializer.TaggedStringElement("group", groupData, "type", groupType));
    }
}

#nullable disable