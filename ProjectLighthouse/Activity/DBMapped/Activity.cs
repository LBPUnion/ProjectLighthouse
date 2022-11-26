using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.RecentActivity;

#nullable enable
public class Activity
{
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
    public int EventId { get; set; }
    public EventType EventType { get; set; }
    public TargetType TargetType { get; set; }
    public int TargetId { get; set; }
    public long EventTimestamp { get; set; }

    public long Interaction1 { get; set; }
    public long Interaction2 { get; set; }

    #nullable disable
    [JsonIgnore]
    public virtual User Actor { get; set; }
    #nullable enable 

    public string Serialize()
    {
        return LbpSerializer.TaggedStringElement("event",
            LbpSerializer.StringElement("timestamp", this.EventTimestamp) +
            LbpSerializer.StringElement("actor", this.Actor.Username) +
            ActivityHelper.ObjectAsEventElement(this.TargetType, this.TargetId, this.database) +
            FormInteraction()
        , "type", ActivityHelper.EventTypeAsString(this.EventType));
    }

    private string FormInteraction()
    {
        switch (this.EventType)
        {
            case EventType.DpadRating:
                return LbpSerializer.StringElement("dpad_rating", this.Interaction1);
            case EventType.LBP1Rate:
                return LbpSerializer.StringElement("rating", this.Interaction1);
            case EventType.Score:
                return LbpSerializer.StringElement("score", this.Interaction1) +
                       LbpSerializer.StringElement("user_count", this.Interaction2);
            case EventType.CommentUser:
            case EventType.CommentLevel:
                return LbpSerializer.StringElement("comment_id", this.Interaction1);
            case EventType.UploadPhoto:
                return LbpSerializer.StringElement("photo_id", this.Interaction1);
            case EventType.PlayLevel:
                return LbpSerializer.StringElement("count", this.Interaction1);
            case EventType.PublishLevel:
                return LbpSerializer.StringElement("republish", this.Interaction1) +
                       LbpSerializer.StringElement("count", this.Interaction2);
            case EventType.Review:
                return LbpSerializer.StringElement("review_id", this.Interaction1) +
                       LbpSerializer.StringElement("review_modified", this.Interaction2);
            default: return "";
        }
    }

    // #nullable enable
    // public async Task<string> SerializeEvent(GameVersion gameVersion)
    // {
    //     switch (TargetType)
    //     {
    //         case TargetType.Level:
    //             Slot? slot = await ActivityHelper.ObjectFinder(this.database, this.TargetType, this.TargetId) as Slot;
    //             if (slot == null) return "";

    //         case TargetType.Profile:
    //         case TargetType.LBP3TeamPick:
    //         case TargetType.News:
    //             return "";
    //     }
    //     return "";
    // }
}