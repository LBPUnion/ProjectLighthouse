using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Serialization.Slot;
using LBPUnion.ProjectLighthouse.Types.Serialization.User;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity;

/// <summary>
/// The global stream object, contains all 
/// </summary>
[XmlRoot("stream")]
public class GameStream : ILbpSerializable, INeedsPreparationForSerialization
{
    [XmlIgnore]
    private List<int> SlotIds { get; set; }

    [XmlIgnore]
    private List<int> UserIds { get; set; }

    [XmlIgnore]
    private int TargetUserId { get; set; }

    [XmlIgnore]
    private GameVersion TargetGame { get; set; }

    [XmlElement("start_timestamp")]
    public long StartTimestamp { get; set; }

    [XmlElement("end_timestamp")]
    public long EndTimestamp { get; set; }

    [XmlArray("groups")]
    [XmlArrayItem("group")]
    public List<GameStreamGroup> Groups { get; set; }

    [XmlArray("slots")]
    [XmlArrayItem("slot")]
    public List<SlotBase> Slots { get; set; }

    [XmlArray("users")]
    [XmlArrayItem("user")]
    public List<GameUser> Users { get; set; }

    [XmlArray("news")]
    [XmlArrayItem("item")]
    public List<object> News { get; set; }
    //TODO implement lbp1 and lbp2 news objects

    public async Task PrepareSerialization(DatabaseContext database)
    {
        if (this.SlotIds.Count > 0)
        {
            this.Slots = new List<SlotBase>();
            foreach (int slotId in this.SlotIds)
            {
                SlotEntity slot = await database.Slots.FindAsync(slotId);
                if (slot == null) continue;

                this.Slots.Add(SlotBase.CreateFromEntity(slot, this.TargetGame, this.TargetUserId));
            }
        }

        if (this.UserIds.Count > 0)
        {
            this.Users = new List<GameUser>();
            foreach (int userId in this.UserIds)
            {
                UserEntity user = await database.Users.FindAsync(userId);
                if (user == null) continue;

                this.Users.Add(GameUser.CreateFromEntity(user, this.TargetGame));
            }
        }
    }

    public static async Task<GameStream> CreateFromEntityResult
    (
        DatabaseContext database,
        GameTokenEntity token,
        List<IGrouping<ActivityGroup, ActivityEntity>> results,
        long startTimestamp,
        long endTimestamp
    )
    {
        List<int> slotIds = results.Where(g => g.Key.TargetSlotId != null && g.Key.TargetSlotId.Value != 0)
            .Select(g => g.Key.TargetSlotId.Value)
            .ToList();
        Console.WriteLine($@"slotIds: {string.Join(",", slotIds)}");
        List<int> userIds = results.Where(g => g.Key.TargetUserId != null && g.Key.TargetUserId.Value != 0)
            .Select(g => g.Key.TargetUserId.Value)
            .Distinct()
            .Union(results.Select(g => g.Key.UserId))
            .ToList();
        // Cache target levels and users within DbContext
        await database.Slots.Where(s => slotIds.Contains(s.SlotId)).LoadAsync();
        await database.Users.Where(u => userIds.Contains(u.UserId)).LoadAsync();
        Console.WriteLine($@"userIds: {string.Join(",", userIds)}");
        Console.WriteLine($@"Stream contains {slotIds.Count} slots and {userIds.Count} users");
        GameStream gameStream = new()
        {
            TargetUserId = token.UserId,
            TargetGame = token.GameVersion,
            StartTimestamp = startTimestamp,
            EndTimestamp = endTimestamp,
            SlotIds = slotIds,
            UserIds = userIds,
            Groups = new List<GameStreamGroup>(),
        };
        foreach (IGrouping<ActivityGroup, ActivityEntity> group in results)
        {
            gameStream.Groups.Add(GameStreamGroup.CreateFromGrouping(group));
        }

        return gameStream;
    }
}