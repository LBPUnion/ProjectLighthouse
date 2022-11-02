using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.EntityFrameworkCore;

#nullable enable

public class ActivitySubject {
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
    public int SubjectId { get; set; }

    public int ActorId { get; set; }
    
    [ForeignKey(nameof(ActorId))]
    public User? Actor { get; set; }
    
    public ActivityType ActivityType { get; set; }

    
    public int ActivityObjectId { get; set; }
    
    public EventType EventType { get; set; }

    public long EventTimestamp { get; set; }

    public int? Interaction { get; set; }
    public long? Interaction2 { get; set; }

    public string Serialize()
    {
        string objectType = ActivityHelper.ObjectType(EventType);

        string action = LbpSerializer.StringElement("timestamp", EventTimestamp) +
                        LbpSerializer.StringElement("actor", Actor?.Username);
        switch (EventType)
        {
            case LBPUnion.ProjectLighthouse.Helpers.EventType.CommentUser:
            case LBPUnion.ProjectLighthouse.Helpers.EventType.HeartUser:
                action += LbpSerializer.StringElement(objectType, Actor?.Username);
                break;
            default:
                action += LbpSerializer.TaggedStringElement(objectType, ActivityObjectId, "type", "user");
                break;
        }
        action += FormInteraction(this);

        return LbpSerializer.TaggedStringElement("event", action, "type", ActivityHelper.EventTypeAsString(EventType));
    }

    private string FormInteraction(ActivitySubject action)
    {
        switch (action.EventType)
        {
            case LBPUnion.ProjectLighthouse.Helpers.EventType.DpadRating:
                return LbpSerializer.StringElement("dpad_rating", action.Interaction);
            case LBPUnion.ProjectLighthouse.Helpers.EventType.LBP1Rate:
                return LbpSerializer.StringElement("rating", action.Interaction);
            case LBPUnion.ProjectLighthouse.Helpers.EventType.Score:
                return LbpSerializer.StringElement("score", action.Interaction) +
                       LbpSerializer.StringElement("user_count", action.Interaction2);
            case LBPUnion.ProjectLighthouse.Helpers.EventType.CommentUser:
            case LBPUnion.ProjectLighthouse.Helpers.EventType.CommentLevel:
                return LbpSerializer.StringElement("comment_id", action.Interaction);
            case LBPUnion.ProjectLighthouse.Helpers.EventType.UploadPhoto:
                return LbpSerializer.StringElement("photo_id", action.Interaction);
            case LBPUnion.ProjectLighthouse.Helpers.EventType.PlayLevel:
                return LbpSerializer.StringElement("count", action.Interaction);
            case LBPUnion.ProjectLighthouse.Helpers.EventType.PublishLevel:
                return LbpSerializer.StringElement("republish", action.Interaction) +
                       LbpSerializer.StringElement("count", action.Interaction2);
            case LBPUnion.ProjectLighthouse.Helpers.EventType.Review:
                return LbpSerializer.StringElement("review_id", action.Interaction) +
                       LbpSerializer.StringElement("review_modified", action.Interaction2);
            default: return "";
        }
    }
}