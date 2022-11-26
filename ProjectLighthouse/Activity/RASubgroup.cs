using System.Collections.Generic;
using System.Text;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.RecentActivity;

public class RASubgroup
{
    public int HostId;
    public string HostUsername = "";
    public TargetType HostType;

    public long Timestamp;
    public string UserId;
    public List<Activity> Events;

    public string SerializeSubgroup()
    {
        StringBuilder eventData = new StringBuilder();

        foreach (Activity activity in Events)
        {
            eventData.Append(activity.Serialize());
        }

        return LbpSerializer.TaggedStringElement("group", 
            LbpSerializer.StringElement("timestamp", this.Timestamp) +
            LbpSerializer.StringElement("user_id", this.UserId) +
            LbpSerializer.StringElement("events", eventData.ToString())
        , "type", "user");
    }
}