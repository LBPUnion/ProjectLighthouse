using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Serialization;

public class ActivityStream {
    [Key]
    public int PostId { get; set; }

    public string PostType { get; set; }

    // Why do the individual items use MS while the stream itself uses seconds?
    public long Timestamp { get; set; } 

    public int ReferencedId { get; set; }

    public int ActorId { get; set; }

    public string Serialize(GameVersion gameVersion = GameVersion.LittleBigPlanet2)
    {
        ActivityGroupType groupType = (PostType == "news_post") ? ActivityGroupType.News : ActivityGroupType.Other;
        string streamData = LbpSerializer.StringElement("timestamp", this.Timestamp);
        if (groupType == ActivityGroupType.News) streamData += LbpSerializer.StringElement("news_id", this.ReferencedId);
        else streamData += LbpSerializer.TaggedStringElement("slot_id", this.ReferencedId, 
                    (groupType == ActivityGroupType.Other) ? "type" : "", (groupType == ActivityGroupType.Other) ? "user" : "");
        streamData += LbpSerializer.StringElement("events", 
            LbpSerializer.TaggedStringElement("event", 
                LbpSerializer.StringElement("timestamp", this.Timestamp) +
                LbpSerializer.TaggedStringElement((groupType == ActivityGroupType.News) ? "news_id" : "object_slot_id", this.ReferencedId, 
                    (groupType == ActivityGroupType.Other) ? "type" : "", (groupType == ActivityGroupType.Other) ? "user" : "")
            , "type", this.PostType)
        );

        return LbpSerializer.TaggedStringElement("group", streamData, "type", this.PostType);
    }
}