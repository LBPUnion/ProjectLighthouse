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
    public int ActionId { get; set; }

    public int ActorId { get; set; }
    
    [ForeignKey(nameof(ActorId))]
    public User? Actor { get; set; }
    
    public int ObjectType { get; set; }

    
    public int ObjectId { get; set; }
    
    public int ActionType { get; set; }

    [NotMapped]
    public ActivityCategory ActionCategory
    {
        get => (ActivityCategory)ActionType;
        set => ActionType = (int)value;
    }
    public long ActionTimestamp { get; set; }

    public int? Interaction { get; set; }
    public long? Interaction2 { get; set; }

    public async Task<string> Serialize()
    {
        string objectType = ActivityHelper.ObjectType(ObjectType);

        string action = LbpSerializer.StringElement("timestamp", ActionTimestamp) +
                        LbpSerializer.StringElement("actor", Actor?.Username);
        switch ((EventType)ObjectType)
        {
            case EventType.CommentUser:
            case EventType.HeartUser:
                User? user = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == ObjectId);
                action += LbpSerializer.StringElement(objectType, user?.Username);
                break;
            default:
                action += LbpSerializer.TaggedStringElement(objectType, ObjectId, "type", "user");
                break;
        }
        action += FormInteraction(this);

        return LbpSerializer.TaggedStringElement("event", action, "type", ActivityHelper.EventTypeAsString(ObjectType));
    }

    private string FormInteraction(ActivitySubject action)
    {
        switch ((EventType)action.ObjectType)
        {
            case EventType.DpadRating:
                return LbpSerializer.StringElement("dpad_rating", action.Interaction);
            case EventType.LBP1Rate:
                return LbpSerializer.StringElement("rating", action.Interaction);
            case EventType.Score:
                return LbpSerializer.StringElement("score", action.Interaction) +
                       LbpSerializer.StringElement("user_count", action.Interaction2);
            case EventType.CommentUser:
            case EventType.CommentLevel:
                return LbpSerializer.StringElement("comment_id", action.Interaction);
            case EventType.UploadPhoto:
                return LbpSerializer.StringElement("photo_id", action.Interaction);
            case EventType.PlayLevel:
                return LbpSerializer.StringElement("count", action.Interaction);
            case EventType.PublishLevel:
                return LbpSerializer.StringElement("republish", action.Interaction) +
                       LbpSerializer.StringElement("count", action.Interaction2);
            case EventType.Review:
                return LbpSerializer.StringElement("review_id", action.Interaction) +
                       LbpSerializer.StringElement("review_modified", action.Interaction2);
            default: return "";
        }
    }
}