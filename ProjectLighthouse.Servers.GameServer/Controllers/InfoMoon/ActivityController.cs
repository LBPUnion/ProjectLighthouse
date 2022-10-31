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
        [FromQuery] bool excludeMyLevels = false,
        [FromQuery] bool excludeFriends = false,
        [FromQuery] bool excludeFavouriteUsers = false,
        [FromQuery] bool excludeMyPlaylists = false
    // [FromQuery] Placeholder groupBy = actorThenObject
    )
    {
        // endTimestamp will report as 0 occasionally, this is automagically handled as 0
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        // I was unable to find a way of getting the user from the GameToken request.
        User? requestee = await this.database.UserFromGameRequest(this.Request);
        if (requestee == null || token == null) return this.StatusCode(403, "");
        if (endTimestamp == 0) endTimestamp = timestamp - 86_400_000; // Get a day's worth of info
        // Used later to determine if the slot in question can be accessed by the requestee.
        GameVersion gameVersion = token.GameVersion;

        IEnumerable<HeartedProfile> requesteeHearts = Enumerable.Empty<HeartedProfile>();

        List<int> idsToResolve = new List<int>();
        if (!excludeMyself) idsToResolve.Add(requestee.UserId);
        IEnumerable<int> heartedUsers = new List<int>();

        if (!excludeFavouriteUsers)
        {
            requesteeHearts = this.database.HeartedProfiles.AsEnumerable().Where(h => h.UserId == requestee.UserId);
            foreach (HeartedProfile hearted in requesteeHearts)
            {
                idsToResolve.Add(hearted.HeartedUserId);
                heartedUsers = heartedUsers.Append(hearted.HeartedUserId);
            }
        }

        IEnumerable<Activity> activities = this.database.Activity
            .AsEnumerable().Where(a =>
                                    (!excludeNews && a.Category == ActivityType.News) ||
                                    (!excludeMyself && a.Users.AsEnumerable().Contains(requestee.UserId)) ||
                                    (!excludeFavouriteUsers && a.Users.AsEnumerable().Intersect(heartedUsers).Any()) ||
                                    (!excludeNews && (a.TargetType == (int)ActivityType.News || 
                                        (a.TargetType == (int)ActivityType.TeamPick && gameVersion == GameVersion.LittleBigPlanet3))
                                    ) ||
                                    ((
                                        a.TargetType == (int)ActivityType.Profile
                                    ) && a.TargetId == requestee.UserId)
                                 );

        string groups = "";
        string slots = "";
        string users = "";
        string news = "";

        foreach (Activity stream in activities.ToList())
        {
            long genericTimestamp = 0;

            string groupData = "";
            string groupType = "";

            List<int> subjectSlotIds = new List<int>();

            bool invalid = false; // AFAIK C# does not support nested continue
            switch (stream.Category)
            {
                case ActivityType.News:
                    News? targetedPost = await this.database.News.FirstOrDefaultAsync(n => n.NewsId == stream.TargetId);
                    if (targetedPost == null) break;
                    news += targetedPost.Serialize();
                    groupType = "news";
                    groupData += LbpSerializer.StringElement("news_id", targetedPost.NewsId);
                    genericTimestamp = targetedPost.Timestamp;
                    break;
                case ActivityType.TeamPick:
                    Slot? targetedPick = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == stream.TargetId);
                    if (targetedPick == null) break;
                    slots += targetedPick.Serialize(gameVersion);
                    groupType = "level";
                    groupData += LbpSerializer.TaggedStringElement("slot_id", targetedPick.SlotId, "type", "user");
                    genericTimestamp = long.Parse(stream.UserCollection); // Cheat if Team Pick
                    break;
                case ActivityType.Level:
                    if (subjectSlotIds.Contains(stream.TargetId)) break;
                    Slot? targetedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == stream.TargetId);
                    if (targetedSlot == null) break;
                    if
                    (
                        token.GameVersion < targetedSlot.GameVersion ||
                        (
                            token.GameVersion == GameVersion.LittleBigPlanetVita &&
                            targetedSlot.GameVersion != GameVersion.LittleBigPlanetVita
                        ) ||
                        (excludeMyLevels && targetedSlot.CreatorId == requestee.UserId) ||
                        (excludeFavouriteUsers && heartedUsers.Contains(targetedSlot.CreatorId)) ||
                        (targetedSlot.SubLevel) // This will be impossible
                    )
                    {
                        invalid = true;
                        break;
                    }
                    subjectSlotIds.Add(stream.TargetId);
                    User? user = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == targetedSlot.CreatorId);
                    if (user == null) break;
                    if (user == requestee)
                    {
                        if (excludeMyLevels)
                        {
                            invalid = true;
                            break;
                        }
                        else
                        {
                            idsToResolve = idsToResolve.Concat(stream.Users).ToList();
                        }
                    }
                    users += user.Serialize(gameVersion);
                    slots += targetedSlot.Serialize(gameVersion);
                    groupType = "level";
                    groupData += LbpSerializer.TaggedStringElement("slot_id", targetedSlot.SlotId, "type", "user");
                    break;
                case ActivityType.Profile:
                    User? targetedUser = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == stream.TargetId);
                    if (targetedUser == null) break;
                    if (targetedUser == requestee)
                    {
                        idsToResolve = idsToResolve.Concat(stream.Users).ToList();
                    }
                    users += targetedUser?.Serialize(gameVersion);
                    groupType = "user";
                    groupData += LbpSerializer.StringElement("user_id", targetedUser?.Username);
                    break;
            }
            if (invalid) continue; // Skip the iteration if the slot is invalid by filter or other reasons


            if (stream.Category != ActivityType.News && stream.Category != ActivityType.TeamPick)
            {
                idsToResolve = idsToResolve.Distinct().ToList();
                List<User> subjectActors = new List<User>();
                foreach (int id in idsToResolve)
                {
                    User? includedUser = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == id);
                    if (includedUser == null) continue;
                    users += includedUser.Serialize(gameVersion);
                    subjectActors.Add(includedUser);
                }

                IEnumerable<ActivitySubject> subjects = this.database.ActivitySubject.Include(a => a.Actor).AsEnumerable()
                    .Where(a => a.ActionTimestamp < timestamp && a.ActionTimestamp > endTimestamp)
                    .Where(a => a.ActionCategory == stream.Category && a.ObjectId == stream.TargetId)
                    .Where(a => idsToResolve.Contains(a.ActorId))
                    .OrderBy(a => a.ActionTimestamp);

                ActivitySubject? catalyst = subjects.FirstOrDefault();
                groupData += LbpSerializer.StringElement("timestamp", catalyst?.ActionTimestamp);

                List<string> subgroupData = new List<string>();

                string subjectData = "";

                int lastType = 0;
                int lastActivity = 0;
                int lastActor = 0;

                string eventData = "";
                foreach (ActivitySubject subject in subjects.ToList())
                {
                    if (subject.ObjectType == (int)EventType.HeartUser && subject.ObjectId != requestee.UserId) continue;
                    if (excludeMyself && subject.ActorId == requestee.UserId) continue;
                    if (lastActor == subject.ActorId)
                    {
                        string tSerialize = await subject.Serialize();
                        eventData += tSerialize;
                    }
                    else
                    {
                        subgroupData.Insert(0,
                            LbpSerializer.TaggedStringElement("group",
                                subjectData + LbpSerializer.StringElement("events", eventData)
                            , "type", "user")
                        );

                        subjectData = LbpSerializer.StringElement("timestamp", subject.ActionTimestamp) +
                                      LbpSerializer.StringElement("user_id", subject.Actor?.Username);

                        string tSerialize = await subject.Serialize();
                        eventData = tSerialize;
                    }
                    lastActivity = subject.ObjectId;
                    lastType = subject.ActionType;
                    lastActor = subject.ActorId;
                }
                subgroupData.Insert(0,
                    LbpSerializer.TaggedStringElement("group",
                        subjectData + LbpSerializer.StringElement("events", eventData)
                    , "type", "user")
                );

                string subgroups = string.Join("", subgroupData);

                groupData += LbpSerializer.StringElement("subgroups", subgroups);
            }
            else if (stream.Category == ActivityType.TeamPick)
            {
                groupData += 
                    LbpSerializer.StringElement("timestamp", genericTimestamp) +
                    LbpSerializer.StringElement("events",
                    LbpSerializer.TaggedStringElement("event", 
                        LbpSerializer.StringElement("timestamp", genericTimestamp) +
                        LbpSerializer.TaggedStringElement("object_slot_id", stream.TargetId, "type", "user")
                    , "type", "mm_pick_level")
                );
            }
            else
            {
                groupData += 
                    LbpSerializer.StringElement("timestamp", genericTimestamp) +
                    LbpSerializer.StringElement("events",
                    LbpSerializer.TaggedStringElement("event", LbpSerializer.StringElement("news_id", stream.TargetId), "type", "news_post"));
            }

            groups = groups.Insert(0, LbpSerializer.TaggedStringElement("group", groupData, "type", groupType));
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