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
        User? requestee = await this.database.UserFromGameRequest(this.Request);
        if (requestee == null || token == null) return this.StatusCode(403, "");
        if (timestamp == 0) return this.Ok(LbpSerializer.BlankElement("stream"));
        GameVersion gameVersion = token.GameVersion;

        IEnumerable<Activity> activities = this.database.Activity
            .AsEnumerable().Where(a => a.Users.AsEnumerable().Contains(requestee.UserId));
        if (excludeNews) activities = activities.Where(a => a.Category != ActivityCategory.News);

        string groups = "";
        string slots = "";
        string users = "";
        string news = "";

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
                .OrderBy(a => a.ActionTimestamp);

            ActivitySubject? catalyst = subjects.FirstOrDefault();
            groupData += LbpSerializer.StringElement("timestamp", catalyst?.ActionTimestamp);

            switch (stream.Category)
            {
                default:
                case ActivityCategory.Level:
                    Slot? targetedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == stream.TargetId);
                    slots += targetedSlot?.Serialize(gameVersion);
                    if (targetedSlot != null)
                    {
                        User? dontBreak = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == targetedSlot.CreatorId);
                        users += dontBreak?.Serialize(gameVersion);
                    }
                    groupType = "level";
                    groupData += LbpSerializer.TaggedStringElement("slot_id", targetedSlot?.SlotId, "type", "user");
                    break;
                case ActivityCategory.User:
                    User? targetedUser = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == stream.TargetId);
                    users += targetedUser?.Serialize(gameVersion);
                    groupType = "user";
                    groupData += LbpSerializer.StringElement("user_id", targetedUser?.Username);
                    break;
            }

            if (stream.Category != ActivityCategory.News)
            {
                List<string> subgroupData = new List<string>();

                string subjectData = "";
                string lastUser = "";
                string eventData = "";
                foreach (ActivitySubject subject in subjects.ToList())
                {
                    User? actor = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(a => a.UserId == subject.ActorId);
                    if (actor == null) continue;

                    if (lastUser == actor.Username)
                    {
                        string waitSerialize = await subject.Serialize();
                        eventData += waitSerialize;
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
                        lastUser = actor.Username;
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

            }

            groups += LbpSerializer.TaggedStringElement("group", groupData, "type", groupType);
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