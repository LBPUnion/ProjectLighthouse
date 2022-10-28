using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.InfoMoon;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ActivityController : ControllerBase
{
    private readonly Database database;
    public ActivityController(Database database)
    {
        this.database = database;
    }

    // LittleBigPlanet InfoMoon
    [HttpGet("news")]
    public async Task<IActionResult> GetLBP1News()
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null || token.GameVersion != GameVersion.LittleBigPlanet1) return this.StatusCode(403, "");

        List<News> newsObject = this.database.News.OrderByDescending(n => n.Timestamp).ToList();

        string newsPrepped = "";
        foreach (News news in newsObject)
        {
            newsPrepped += news.Serialize(token.GameVersion);
        }

        return this.Ok(LbpSerializer.StringElement("news", newsPrepped));
    }

    [HttpGet("stream/slot/{slotId}")]
    public async Task<IActionResult> GetSlotActivity()
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        User? requestee = await this.database.UserFromGameRequest(this.Request);
        if (requestee == null || token == null) return this.StatusCode(403, "");

        // STUB

        return this.Ok(LbpSerializer.BlankElement("stream"));
    }

    [HttpGet("stream2/{userId}")]
    public async Task<IActionResult> GetUserActivity()
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        User? requestee = await this.database.UserFromGameRequest(this.Request);
        if (requestee == null || token == null) return this.StatusCode(403, "");

        // STUB

        return this.Ok(LbpSerializer.BlankElement("stream"));
    }

    // This function gets global activity that is used on the Recent Activity
    [HttpPost("stream")]
    [HttpGet("stream")]
    public async Task<IActionResult> GetActivity
    (
        [FromQuery] long timestamp,
        [FromQuery] long endTimestamp,
        [FromQuery] bool excludeNews,
        [FromQuery] bool excludeMyself,
        [FromQuery] bool excludeMyPlaylists = false
    // [FromQuery] Placeholder groupBy = actorThenObject
    )
    {
        // endTimestamp will report as 0 occasionally, this is automagically handled as 0
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        // I was unable to find a way of getting the user from the GameToken request.
        User? requestee = await this.database.UserFromGameRequest(this.Request);
        if (requestee == null || token == null) return this.StatusCode(403, "");
        // LBP3 will send a second request for stream objects sent specifically at the beginning of time (0 ms since 1970), 
        // if it does not receive a 200, it will start rapidly spamming requests. Thanks LBP3!
        if (timestamp == 0) return this.Ok(LbpSerializer.BlankElement("stream"));
        // Used later to determine if the slot in question can be accessed by the requestee.
        GameVersion gameVersion = token.GameVersion;

        IEnumerable<Activity> activities = this.database.Activity
            .AsEnumerable().Where(a => a.Users.AsEnumerable().Contains(requestee.UserId) || 
                                    // Remove if filters out self
                                    ((
                                        a.TargetType == (int)ActivityCategory.User ||
                                        a.TargetType == (int)ActivityCategory.HeartUser
                                    ) &&
                                    a.TargetId == requestee.UserId)
                                 );

        if (excludeNews) activities = activities.Where(a => a.Category != ActivityCategory.News);

        string groups = "";
        string slots = "";
        string users = "";
        string news = "";

        long lastActTimestamp = 0;
        long newActTimestamp = 0;
        foreach (Activity stream in activities.ToList())
        {
            List<int> idsToResolve = new List<int>();
            if (!excludeMyself) idsToResolve.Add(requestee.UserId);

            IEnumerable<HeartedProfile> requesteeHearts = this.database.HeartedProfiles.AsEnumerable().Where(h => h.UserId == requestee.UserId);
            foreach (HeartedProfile hearted in requesteeHearts)
            {
                idsToResolve.Add(hearted.HeartedUserId);
            }

            foreach (int id in idsToResolve)
            {
                User? includedUser = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == id);
                users += includedUser?.Serialize(gameVersion);
            }

            string groupData = "";
            string groupType = "";

            IEnumerable<ActivitySubject> subjects = this.database.ActivitySubject.Include(a => a.Actor).AsEnumerable()
                .Where(a => a.ActionTimestamp < timestamp && a.ActionTimestamp > endTimestamp)
                .Where(a => a.ActionCategory == stream.Category && a.ObjectId == stream.TargetId)
                .Where(a => idsToResolve.Contains(a.ActorId))
                .OrderByDescending(a => a.ActionTimestamp);

            ActivitySubject? catalyst = subjects.FirstOrDefault();
            groupData += LbpSerializer.StringElement("timestamp", catalyst?.ActionTimestamp);

            bool invalid = false; // AFAIK C# does not support nested continue
            switch (stream.Category)
            {
                default:
                case ActivityCategory.Level:
                    Slot? targetedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == stream.TargetId);
                    if 
                    (
                        token.GameVersion < targetedSlot?.GameVersion || 
                        (
                            (token.GameVersion == GameVersion.LittleBigPlanetVita || token.GameVersion == GameVersion.LittleBigPlanetPSP) && 
                            (targetedSlot?.GameVersion != GameVersion.LittleBigPlanetVita || targetedSlot.GameVersion != GameVersion.LittleBigPlanetPSP)
                        )
                    )
                    {
                        invalid = true;
                        break;
                    }
                    slots += targetedSlot?.Serialize(gameVersion);
                    if (targetedSlot != null)
                    {
                        User? dontBreak = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == targetedSlot.CreatorId);
                        users += dontBreak?.Serialize(gameVersion);
                    }
                    groupType = "level";
                    groupData += LbpSerializer.TaggedStringElement("slot_id", targetedSlot?.SlotId, "type", "user");
                    break;
                case ActivityCategory.HeartUser:
                case ActivityCategory.User:
                    User? targetedUser = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == stream.TargetId);
                    users += targetedUser?.Serialize(gameVersion);
                    groupType = "user";
                    groupData += LbpSerializer.StringElement("user_id", targetedUser?.Username);
                    break;
            }
            if (invalid) continue;

            if (stream.Category != ActivityCategory.News)
            {
                List<string> subgroupData = new List<string>();

                string subjectData = "";

                int lastType = 0;
                int lastActivity = 0;
                int lastActor = 0;

                string eventData = "";
                foreach (ActivitySubject subject in subjects.ToList())
                {
                    newActTimestamp = Math.Max(subject.ActionTimestamp, newActTimestamp);
                    User? actor = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(a => a.UserId == subject.ActorId);
                    if (actor == null) continue;

                    if (subject.ActionType == (int)ActivityCategory.HeartUser)
                    {
                        subgroupData.Insert(0,
                            LbpSerializer.TaggedStringElement("group",
                                subjectData + LbpSerializer.StringElement("events", eventData)
                            , "type", "user")
                        );
                        if (actor == requestee) continue; // Ignore if heart user actor is the requestee
                        // DO. NOT. COMBINE. THESE.
                        subjectData = LbpSerializer.StringElement("timestamp", subject.ActionTimestamp) +
                                      LbpSerializer.StringElement("user_id", actor.Username);
                        lastActivity = subject.ObjectId;
                        lastType = subject.ActionType;
                        lastActor = subject.ActorId;

                        string waitSerialize = await subject.Serialize();
                        eventData = waitSerialize;
                        continue;
                    }
                    if (lastActivity == subject.ObjectId && lastType == subject.ActionType &&
                        (lastActor == subject.ActorId || 
                            (token.GameVersion == GameVersion.LittleBigPlanet3 ? 
                                subject.ActionType == (int)ActivityCategory.Comment : false
                            )
                        )
                    )
                    {
                        string waitSerialize = await subject.Serialize();
                        eventData += waitSerialize;
                    }
                    else if (lastActivity == subject.ObjectId && lastType == subject.ActionType && lastActor != subject.ActorId)
                    {
                        subgroupData.Insert(0,
                            LbpSerializer.TaggedStringElement("group",
                                subjectData + LbpSerializer.StringElement("events", eventData)
                            , "type", "user")
                        );

                        subjectData = LbpSerializer.StringElement("timestamp", subject.ActionTimestamp) +
                                      LbpSerializer.StringElement("user_id", actor.Username);

                        lastActivity = subject.ObjectId;
                        lastType = subject.ActionType;
                        lastActor = subject.ActorId;

                        string waitSerialize = await subject.Serialize();
                        eventData = waitSerialize;
                    }
                    else
                    {
                        if (eventData != "")
                        {
                            subgroupData.Insert(0,
                                LbpSerializer.TaggedStringElement("group",
                                    subjectData + LbpSerializer.StringElement("events", eventData)
                                , "type", "user")
                            );
                        }

                        subjectData = LbpSerializer.StringElement("timestamp", subject.ActionTimestamp) +
                                      LbpSerializer.StringElement("user_id", actor.Username);
                        lastActivity = subject.ObjectId;
                        lastType = subject.ActionType;
                        lastActor = subject.ActorId;

                        string waitSerialize = await subject.Serialize();
                        eventData = waitSerialize;
                    }
                }
                subgroupData.Insert(0,
                    LbpSerializer.TaggedStringElement("group",
                        subjectData + LbpSerializer.StringElement("events", eventData)
                    , "type", "user")
                );

                string subgroups = string.Join("", subgroupData);

                groupData += LbpSerializer.StringElement("subgroups", subgroups);
            }
            else
            {
                // News Stub
            }

            groups = groups.Insert(0, LbpSerializer.TaggedStringElement("group", groupData, "type", groupType));
            lastActTimestamp = newActTimestamp;
        }

        return this.Ok(
            LbpSerializer.StringElement("stream",
                LbpSerializer.StringElement("start_timestamp", timestamp) +
                LbpSerializer.StringElement("end_timestamp", endTimestamp) +
                LbpSerializer.StringElement("groups", groups) +
                ((slots != "") ? LbpSerializer.StringElement("slots", slots) : "") +
                ((users != "") ? LbpSerializer.StringElement("users", users) : "") +
                ((news != "") ? LbpSerializer.StringElement("news", news) : "")
            )
        );
    }
}