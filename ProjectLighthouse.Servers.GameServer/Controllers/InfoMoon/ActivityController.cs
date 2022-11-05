using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
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
        if (endTimestamp == 0) endTimestamp = timestamp - 86_400_000; // Get a day's worth of info, the bigger the number, the greater the performance hit.
        // Used later to determine if the slot in question can be accessed by the requestee.
        GameVersion gameVersion = token.GameVersion;

        IEnumerable<HeartedProfile> requesteeHearts = Enumerable.Empty<HeartedProfile>();
        UserFriendData? requesteeFriends = new UserFriendData();

        List<int> heartedUsers = new List<int>();
        List<int> friendUsers = new List<int>();

        /* 
            Workaround to avoid massive performance loss.
            EntityFramework does not support FIND_IN_SET, and thus we are required to use the raw SQL command, FIND_IN_SET
            TODO: Investigate possibility of SQL Injection, this is VERY important to pay attention to! 
                  Although SqlInterpolated mitigates this risk, it is imperative we ensure this is impossible.

            Additionally, from what I am able to tell, MySQL, MariaDB, and other databases using RDBMS don't care much when 
            using multiple small statements.
        */
        IEnumerable<Activity> activities = this.database.Activity.Where(a => a.ActivityType == ActivityType.Profile && a.ActivityTargetId == requestee.UserId);

        if (!excludeNews)
        {
            activities = activities.Where(a => a.ActivityType == ActivityType.News);
            if (gameVersion == GameVersion.LittleBigPlanet3)
            {
                activities = activities.Where(a => a.ActivityType == ActivityType.TeamPick);
            }
        }

        DbSet<Activity> actContext = this.database.Activity;

        if (!excludeMyLevels)
        {
            activities = activities.Concat(actContext.FromSqlInterpolated($"SELECT * FROM Activity WHERE SUBSTRING_INDEX(ExtrasCollection, ',', 1) = {requestee.UserId}"));
        }
        if (!excludeMyself)
        {
            activities = activities.Concat(actContext.FromSqlInterpolated($"SELECT * FROM Activity WHERE FIND_IN_SET({requestee.UserId}, ExtrasCollection)"));
        }

        if (!excludeFavouriteUsers)
        {
            requesteeHearts = this.database.HeartedProfiles.Where(h => h.UserId == requestee.UserId);
            foreach (HeartedProfile hearted in requesteeHearts)
            {
                heartedUsers.Add(hearted.HeartedUserId);
                activities = activities.Concat(actContext.FromSqlInterpolated($"SELECT * FROM Activity WHERE FIND_IN_SET({hearted.HeartedUserId}, ExtrasCollection)"));
            }
        }

        if (!excludeFriends)
        {
            requesteeFriends = UserFriendStore.GetUserFriendData(requestee.UserId);
            if (requesteeFriends != null)
            {
                foreach (int id in requesteeFriends.FriendIds)
                {
                    friendUsers.Add(id);
                    activities = activities.Concat(actContext.FromSqlInterpolated($"SELECT * FROM Activity WHERE FIND_IN_SET({id}, ExtrasCollection)"));
                }
            }
        }

        // Make sure each request only appears once, execute, make sure each activity slot is unique
        activities = activities.Distinct().OrderBy(a => a.ActivityId);

        string groups = "";
        string slots = "";
        string users = "";
        string news = "";

        foreach (Activity stream in activities.ToList())
        {
            List<int> idsToResolve = new List<int>();
            if (!excludeMyself) idsToResolve.Add(requestee.UserId);

            if (!excludeFavouriteUsers)
            {
                foreach (int id in heartedUsers)
                {
                    idsToResolve.Add(id);
                }
            }
            if (!excludeFriends)
            {
                foreach (int id in friendUsers)
                {
                    idsToResolve.Add(id);
                }
            }

            long genericTimestamp = 0;

            string groupData = "";
            string groupType = "";

            List<int> subjectSlotIds = new List<int>();

            bool invalid = false; // AFAIK C# does not support nested continue
            switch (stream.ActivityType)
            {
                case ActivityType.News:
                    News? targetedPost = await this.database.News.FirstOrDefaultAsync(n => n.NewsId == stream.ActivityTargetId);
                    if (targetedPost == null) break;
                    if (heartedUsers.Contains(targetedPost.CreatorId) && !excludeFavouriteUsers && excludeNews) break;
                    news += targetedPost.Serialize();
                    groupType = "news";
                    groupData += LbpSerializer.StringElement("news_id", targetedPost.NewsId);
                    genericTimestamp = targetedPost.Timestamp;
                    break;
                case ActivityType.TeamPick:
                    Slot? targetedPick = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == stream.ActivityTargetId);
                    if (targetedPick == null) break;
                    slots += targetedPick.Serialize(gameVersion);
                    groupType = "level";
                    groupData += LbpSerializer.TaggedStringElement("slot_id", targetedPick.SlotId, "type", "user");
                    genericTimestamp = long.Parse(stream.ExtrasCollection); // Cheat if Team Pick
                    break;
                case ActivityType.Level:
                    if (subjectSlotIds.Contains(stream.ActivityTargetId)) break;
                    Slot? targetedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == stream.ActivityTargetId);
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
                    subjectSlotIds.Add(stream.ActivityTargetId);
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
                            idsToResolve = idsToResolve.Concat(stream.Extras).ToList();
                        }
                    }
                    users += user.Serialize(gameVersion);
                    slots += targetedSlot.Serialize(gameVersion);
                    groupType = "level";
                    groupData += LbpSerializer.TaggedStringElement("slot_id", targetedSlot.SlotId, "type", "user");
                    break;
                case ActivityType.Profile:
                    User? targetedUser = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == stream.ActivityTargetId);
                    if (targetedUser == null) break;
                    if (targetedUser == requestee)
                    {
                        idsToResolve = idsToResolve.Concat(stream.Extras).ToList();
                    }
                    users += targetedUser?.Serialize(gameVersion);
                    groupType = "user";
                    groupData += LbpSerializer.StringElement("user_id", targetedUser?.Username);
                    break;
            }
            if (invalid) continue; // Skip the iteration if the slot is invalid by filter or other reasons


            if (stream.ActivityType != ActivityType.News && stream.ActivityType != ActivityType.TeamPick)
            {
                idsToResolve = idsToResolve.Distinct().ToList();
                List<User> subjectActors = new List<User>();

                IEnumerable<ActivitySubject> subjects = Enumerable.Empty<ActivitySubject>();
                foreach (int id in idsToResolve)
                {
                    User? includedUser = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == id);
                    if (includedUser == null) continue;
                    users += includedUser.Serialize(gameVersion);
                    subjectActors.Add(includedUser);

                    subjects = subjects.Concat(
                        this.database.ActivitySubject.FromSqlInterpolated(
                            $"SELECT * FROM ActivitySubject WHERE EventTimestamp < {timestamp} and EventTimestamp > {endTimestamp} and ActivityType = {stream.ActivityType} and ActivityObjectId = {stream.ActivityTargetId} and FIND_IN_SET({id}, ActorId)"
                        )
                    );
                }
                subjects = subjects.AsEnumerable().OrderBy(a => a.EventTimestamp);

                ActivitySubject? catalyst = subjects.FirstOrDefault();
                groupData += LbpSerializer.StringElement("timestamp", catalyst?.EventTimestamp);

                List<string> subgroupData = new List<string>();

                string subjectData = "";

                EventType lastType = 0;
                int lastActivity = 0;
                int lastActor = 0;

                string eventData = "";
                foreach (ActivitySubject subject in subjects.ToList())
                {
                    if (subject.EventType == EventType.HeartUser && subject.ActivityObjectId != requestee.UserId) continue;
                    if (excludeMyself && subject.ActorId == requestee.UserId) continue;
                    if (lastActor == subject.ActorId)
                    {
                        string tSerialize = subject.Serialize();
                        eventData += tSerialize;
                    }
                    else
                    {
                        subgroupData.Insert(0,
                            LbpSerializer.TaggedStringElement("group",
                                subjectData + LbpSerializer.StringElement("events", eventData)
                            , "type", "user")
                        );

                        subjectData = LbpSerializer.StringElement("timestamp", subject.EventTimestamp) +
                                      LbpSerializer.StringElement("user_id", subject.Actor?.Username);

                        string tSerialize = subject.Serialize();
                        eventData = tSerialize;
                    }
                    lastActivity = subject.ActivityObjectId;
                    lastType = subject.EventType;
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
            else if (stream.ActivityType == ActivityType.TeamPick)
            {
                groupData +=
                    LbpSerializer.StringElement("timestamp", genericTimestamp) +
                    LbpSerializer.StringElement("events",
                    LbpSerializer.TaggedStringElement("event",
                        LbpSerializer.StringElement("timestamp", genericTimestamp) +
                        LbpSerializer.TaggedStringElement("object_slot_id", stream.ActivityTargetId, "type", "user")
                    , "type", "mm_pick_level")
                );
            }
            else
            {
                groupData +=
                    LbpSerializer.StringElement("timestamp", genericTimestamp) +
                    LbpSerializer.StringElement("events",
                    LbpSerializer.TaggedStringElement("event", LbpSerializer.StringElement("news_id", stream.ActivityTargetId), "type", "news_post"));
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