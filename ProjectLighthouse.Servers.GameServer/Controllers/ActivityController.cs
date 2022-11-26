using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LBPUnion.ProjectLighthouse.RecentActivity;
using System.Text;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

// Prototyping

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Route("debug/")]
[Produces("text/xml")]
public class ActivityController : ControllerBase
{
    private readonly Database database;

    public ActivityController(Database _database)
    {
        database = _database;
    }

    [HttpGet("stream/user2/{username}")]
    public IActionResult GetUserStream([FromQuery] long timestamp, string username)
    {
        // GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        // if (token == null) return this.Unauthorized();
        long endTimestamp = timestamp - 86_400_000;  // 1 Day

        User? userTarget = this.database.Users.Include(u => u.Location).Include(u => u.PlayerEvents.Where(e => e.EventTimestamp <= timestamp && e.EventTimestamp >= endTimestamp)).FirstOrDefault(u => u.Username == username);
        if (userTarget == null) return this.BadRequest();

        List<User> users = new List<User>() {
            userTarget
        };
        
        return this.Ok(StreamBuilder(users, timestamp, endTimestamp));
    }

    [HttpGet("stream/slot/user/{slotId}")]
    public IActionResult GetSlotStream([FromQuery] long timestamp, int slotId)
    {
        long endTimestamp = timestamp - 86_400_000; // 1 Day

        Slot? slotTarget = this.database.Slots.FirstOrDefault(s => s.SlotId == slotId);
        if (slotTarget == null) return this.BadRequest();

        return this.Ok(StreamBuilder(slotTarget.SlotId, timestamp, endTimestamp));
    }

    [HttpGet("stream")]
    [HttpPost("stream")]
    public async Task<IActionResult> GetGlobalStream([FromQuery] long timestamp) // I'll put filters here later
    {
        long endTimestamp = timestamp - 86_400_000; // 1 Day
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.BadRequest();
        User? user = this.database.Users.Include(u => u.PlayerEvents.Where(e => e.EventTimestamp <= timestamp && e.EventTimestamp >= endTimestamp)).FirstOrDefault(u => u.UserId == token.UserId);
        if (user == null) return this.BadRequest();

        return this.Ok(StreamBuilder(new List<User>() {user}, timestamp, endTimestamp));
    }

    private string StreamBuilder(List<User> userTargets, long timestamp, long endTimestamp)
    {
        IEnumerable<Activity> playerEvents = Enumerable.Empty<Activity>();

        foreach (User userTarget in userTargets ) playerEvents = playerEvents.Concat(userTarget.PlayerEvents.AsEnumerable());

        return Build(playerEvents, timestamp, endTimestamp);
    }

    private string StreamBuilder(int slotTarget, long timestamp, long endTimestamp)
    {
        IEnumerable<Activity> playerEvents = this.database.Activity.Include(a => a.Actor).Where(a => a.TargetType == TargetType.Level).Where(a => a.TargetId == slotTarget);
        return Build(playerEvents, timestamp, endTimestamp);
    }

    private string Build(IEnumerable<Activity> playerEvents, long timestamp, long endTimestamp)
    {
        StringBuilder returnText = new StringBuilder();
        returnText.Append(LbpSerializer.StringElement("start_timestamp", timestamp));
        returnText.Append(LbpSerializer.StringElement("end_timestamp", endTimestamp));

        StringBuilder groups = new StringBuilder();
        string objects = "";

        List<Slot> slots = new List<Slot>();
        List<User> users = new List<User>();

        List<RASubgroup> subgroups = new List<RASubgroup>();
        foreach(Activity activity in playerEvents.OrderByDescending(e => e.EventTimestamp))
        {
            User? actor = this.database.Users.Include(u => u.Location).FirstOrDefault(u => u.UserId == activity.Actor.UserId);
            if (actor == null) continue;
            users.Add(actor);
            RASubgroup? subgroup = subgroups
                .Where(s => s.HostId == activity.TargetId)
                .Where(s => s.UserId == activity.Actor.Username)
                .FirstOrDefault();
            if 
            (   
                subgroup == null || 
                activity.TargetId != subgroup.HostId || 
                activity.TargetType != subgroup.HostType
            )
            {
                RASubgroup newSubgroup = new RASubgroup();
                newSubgroup.HostType = activity.TargetType;
                newSubgroup.HostId = activity.TargetId;
                if (activity.TargetType == TargetType.Profile)
                {
                    User? user =  this.database.Users.Include(u => u.Location).FirstOrDefault(u => u.UserId == activity.TargetId);
                    if (user == null) continue;
                    newSubgroup.HostUsername = user.Username;
                }
                newSubgroup.Timestamp = activity.EventTimestamp;
                newSubgroup.UserId = activity.Actor.Username;
                newSubgroup.Events = new List<Activity>();
                newSubgroup.Events.Add(activity);
                subgroups.Add(newSubgroup);
            }
            else
            {
                if (subgroup.Events == null) subgroup.Events = new List<Activity>();
                subgroup.Events.Add(activity);
                int index = subgroups.IndexOf(subgroup);
                subgroups[index].Events = subgroup.Events;
            }
        }

        users = users.Distinct().ToList();

        foreach(RASubgroup subgroup in subgroups.OrderByDescending(s => s.Timestamp))
        {
            object? obj = ActivityHelper.ObjectFinder(this.database, subgroup.HostType, subgroup.HostId);
            if (obj == null) continue;
            if (obj as User != null) users.Add(obj as User);
            else if (obj as Slot != null) slots.Add(obj as Slot);
            string element;
            if (subgroup.HostType == TargetType.Profile)
            {
                element = LbpSerializer.StringElement("user_id", subgroup.HostUsername);
            }
            else
            {
                element = LbpSerializer.TaggedStringElement("slot_id", subgroup.HostId, "type", "user");
            }
            groups.Append(
                LbpSerializer.TaggedStringElement("group",
                    LbpSerializer.StringElement("timestamp", subgroup.Timestamp) +
                    element +
                    LbpSerializer.StringElement("subgroups", subgroup.SerializeSubgroup())
                , "type", (subgroup.HostType == TargetType.Level ? "level" : "user"))
            );
        }

        string usrstaging = "";
        foreach(User user in users.Distinct())
        {
            usrstaging += user.Serialize(GameVersion.LittleBigPlanet3);
        }

        string slotstaging = "";
        foreach(Slot slot in slots.Distinct())
        {
            slotstaging += slot.Serialize(GameVersion.LittleBigPlanet2);
        }
        objects = LbpSerializer.StringElement("slots", slotstaging) + LbpSerializer.StringElement("users", usrstaging);

        return LbpSerializer.StringElement("stream", returnText.ToString() + LbpSerializer.StringElement("groups", groups.ToString()) + objects);
    }
}