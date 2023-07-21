using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Serialization.Activity;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/stream")]
[Produces("text/xml")]
public class ActivityController : ControllerBase
{
    private readonly DatabaseContext database;

    public ActivityController(DatabaseContext database)
    {
        this.database = database;
    }

    public class ActivityDto
    {
        public required ActivityEntity Activity { get; set; }
        public int? TargetSlotId { get; set; }
        public int? TargetUserId { get; set; }
        public int? TargetPlaylistId { get; set; }
        public int? SlotCreatorId { get; set; }
    }
    //TODO refactor this mess into a separate db file or something

    private static Expression<Func<ActivityEntity, ActivityDto>> ActivityToDto()
    {
        return a => new ActivityDto
        {
            Activity = a,
            TargetSlotId = a is LevelActivityEntity
                ? ((LevelActivityEntity)a).SlotId
                : a is PhotoActivityEntity && ((PhotoActivityEntity)a).Photo.PhotoId != 0
                    ? ((PhotoActivityEntity)a).Photo.SlotId
                    : a is CommentActivityEntity && ((CommentActivityEntity)a).Comment.Type == CommentType.Level
                        ? ((CommentActivityEntity)a).Comment.TargetId
                        : a is ScoreActivityEntity
                            ? ((ScoreActivityEntity)a).Score.SlotId
                            : 0,

            TargetUserId = a is UserActivityEntity
                ? ((UserActivityEntity)a).TargetUserId
                : a is CommentActivityEntity && ((CommentActivityEntity)a).Comment.Type == CommentType.Profile
                    ? ((CommentActivityEntity)a).Comment.TargetId
                    : a is PhotoActivityEntity && ((PhotoActivityEntity)a).Photo.SlotId != 0
                        ? ((PhotoActivityEntity)a).Photo.CreatorId
                        : 0,
            TargetPlaylistId = a is PlaylistActivityEntity ? ((PlaylistActivityEntity)a).PlaylistId : 0,
        };
    }

    private static IQueryable<IGrouping<ActivityGroup, ActivityEntity>> GroupActivities
        (IQueryable<ActivityEntity> activityQuery)
    {
        return activityQuery.Select(ActivityToDto())
            .GroupBy(dto => new ActivityGroup
                {
                    Timestamp = dto.Activity.Timestamp.Date,
                    UserId = dto.Activity.UserId,
                    TargetUserId = dto.TargetUserId,
                    TargetSlotId = dto.TargetSlotId,
                    TargetPlaylistId = dto.TargetPlaylistId,
                },
                dto => dto.Activity);
    }

    private static IQueryable<IGrouping<ActivityGroup, ActivityEntity>> GroupActivities
        (IQueryable<ActivityDto> activityQuery)
    {
        return activityQuery.GroupBy(dto => new ActivityGroup
            {
                Timestamp = dto.Activity.Timestamp.Date,
                UserId = dto.Activity.UserId,
                TargetUserId = dto.TargetUserId,
                TargetSlotId = dto.TargetSlotId,
                TargetPlaylistId = dto.TargetPlaylistId,
            },
            dto => dto.Activity);
    }

    // TODO this is kinda ass, can maybe improve once comment migration is merged
    private async Task<IQueryable<ActivityEntity>> GetFilters
    (
        GameTokenEntity token,
        bool excludeNews,
        bool excludeMyLevels,
        bool excludeFriends,
        bool excludeFavouriteUsers,
        bool excludeMyself
    )
    {
        IQueryable<ActivityEntity> query = this.database.Activities.AsQueryable();
        if (excludeNews) query = query.Where(a => a.Type != EventType.NewsPost);

        IQueryable<ActivityDto> dtoQuery = query.Select(a => new ActivityDto
        {
            Activity = a,
            SlotCreatorId = a is LevelActivityEntity
                ? ((LevelActivityEntity)a).Slot.CreatorId
                : a is PhotoActivityEntity && ((PhotoActivityEntity)a).Photo.SlotId != 0
                    ? ((PhotoActivityEntity)a).Photo.Slot!.CreatorId
                    : a is CommentActivityEntity && ((CommentActivityEntity)a).Comment.Type == CommentType.Level
                        ? ((CommentActivityEntity)a).Comment.TargetId
                        : a is ScoreActivityEntity
                            ? ((ScoreActivityEntity)a).Score.Slot.CreatorId
                            : 0,
        });

        Expression<Func<ActivityDto, bool>> predicate = PredicateExtensions.False<ActivityDto>();

        predicate = predicate.Or(a => a.SlotCreatorId == 0 || excludeMyLevels
            ? a.SlotCreatorId != token.UserId
            : a.SlotCreatorId == token.UserId);

        List<int>? friendIds = UserFriendStore.GetUserFriendData(token.UserId)?.FriendIds;
        if (friendIds != null)
        {
            predicate = excludeFriends
                ? predicate.Or(a => !friendIds.Contains(a.Activity.UserId))
                : predicate.Or(a => friendIds.Contains(a.Activity.UserId));
        }

        List<int> favouriteUsers = await this.database.HeartedProfiles.Where(hp => hp.UserId == token.UserId)
            .Select(hp => hp.HeartedUserId)
            .ToListAsync();

        predicate = excludeFavouriteUsers
            ? predicate.Or(a => !favouriteUsers.Contains(a.Activity.UserId))
            : predicate.Or(a => favouriteUsers.Contains(a.Activity.UserId));

        predicate = excludeMyself
            ? predicate.Or(a => a.Activity.UserId != token.UserId)
            : predicate.Or(a => a.Activity.UserId == token.UserId);

        query = dtoQuery.Where(predicate).Select(dto => dto.Activity);

        return query.OrderByDescending(a => a.Timestamp);
    }

    public Task<DateTime> GetMostRecentEventTime(GameTokenEntity token, DateTime upperBound)
    {
        return this.database.Activities.Where(a => a.UserId == token.UserId)
            .Where(a => a.Timestamp < upperBound)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => a.Timestamp)
            .FirstOrDefaultAsync();
    }

    [HttpGet]
    public async Task<IActionResult> GlobalActivity
    (
        long timestamp,
        bool excludeNews,
        bool excludeMyLevels,
        bool excludeFriends,
        bool excludeFavouriteUsers,
        bool excludeMyself
    )
    {
        GameTokenEntity token = this.GetToken();

        if (token.GameVersion == GameVersion.LittleBigPlanet1) return this.BadRequest();

        if (timestamp > TimeHelper.TimestampMillis || timestamp <= 0) timestamp = TimeHelper.TimestampMillis;

        DateTime start = DateTimeExtensions.FromUnixTimeMilliseconds(timestamp);

        DateTime soonestTime = await this.GetMostRecentEventTime(token, start);
        Console.WriteLine(@"Most recent event occurred at " + soonestTime);
        soonestTime = soonestTime.Subtract(TimeSpan.FromDays(1));

        long soonestTimestamp = soonestTime.ToUnixTimeMilliseconds();
        
        long endTimestamp = soonestTimestamp - 86_400_000;

        Console.WriteLine(@$"soonestTime: {soonestTimestamp}, endTime: {endTimestamp}");

        IQueryable<ActivityEntity> activityEvents = await this.GetFilters(token,
            excludeNews,
            excludeMyLevels,
            excludeFriends,
            excludeFavouriteUsers,
            excludeMyself);

        DateTime end = DateTimeExtensions.FromUnixTimeMilliseconds(endTimestamp);

        activityEvents = activityEvents.Where(a => a.Timestamp < start && a.Timestamp > end);

        Console.WriteLine($@"start: {start}, end: {end}");

        List<IGrouping<ActivityGroup, ActivityEntity>> groups = await GroupActivities(activityEvents).ToListAsync();

        foreach (IGrouping<ActivityGroup, ActivityEntity> group in groups)
        {
            ActivityGroup key = group.Key;
            Console.WriteLine(
                $@"{key.GroupType}: Timestamp: {key.Timestamp}, UserId: {key.UserId}, TargetSlotId: {key.TargetSlotId}, " +
                @$"TargetUserId: {key.TargetUserId}, TargetPlaylistId: {key.TargetPlaylistId}");
            foreach (ActivityEntity activity in group)
            {
                Console.WriteLine($@"  {activity.Type}: Timestamp: {activity.Timestamp}");
            }
        }

        DateTime oldestTime = groups.Any() ? groups.Min(g => g.Any() ? g.Min(a => a.Timestamp) : end) : end;
        long oldestTimestamp = oldestTime.ToUnixTimeMilliseconds();

        return this.Ok(await GameStream.CreateFromEntityResult(this.database, token, groups, timestamp, oldestTimestamp));
    }

    [HttpGet("slot/{slotType}/{slotId:int}")]
    public async Task<IActionResult> SlotActivity(string slotType, int slotId, long timestamp)
    {
        GameTokenEntity token = this.GetToken();

        if (token.GameVersion == GameVersion.LittleBigPlanet1) return this.BadRequest();

        if (timestamp > TimeHelper.TimestampMillis || timestamp <= 0) timestamp = TimeHelper.TimestampMillis;

        long endTimestamp = timestamp - 864_000;

        if (slotType is not ("developer" or "user")) return this.BadRequest();

        if (slotType == "developer")
            slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        IQueryable<ActivityDto> slotActivity = this.database.Activities.Select(ActivityToDto())
            .Where(a => a.TargetSlotId == slotId);

        DateTime start = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
        DateTime end = DateTimeOffset.FromUnixTimeMilliseconds(endTimestamp).DateTime;

        slotActivity = slotActivity.Where(a => a.Activity.Timestamp < start && a.Activity.Timestamp > end);

        List<IGrouping<ActivityGroup, ActivityEntity>> groups = await GroupActivities(slotActivity).ToListAsync();

        DateTime oldestTime = groups.Max(g => g.Max(a => a.Timestamp));
        long oldestTimestamp = new DateTimeOffset(oldestTime).ToUnixTimeMilliseconds();

        return this.Ok(await GameStream.CreateFromEntityResult(this.database, token, groups, timestamp, oldestTimestamp));
    }

    [HttpGet("user2/{userId:int}/")]
    public async Task<IActionResult> UserActivity(int userId, long timestamp)
    {
        GameTokenEntity token = this.GetToken();

        if (token.GameVersion == GameVersion.LittleBigPlanet1) return this.BadRequest();

        if (timestamp > TimeHelper.TimestampMillis || timestamp <= 0) timestamp = TimeHelper.TimestampMillis;

        long endTimestamp = timestamp - 864_000;

        IQueryable<ActivityDto> userActivity = this.database.Activities.Select(ActivityToDto())
            .Where(a => a.TargetUserId == userId);

        DateTime start = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
        DateTime end = DateTimeOffset.FromUnixTimeMilliseconds(endTimestamp).DateTime;

        userActivity = userActivity.Where(a => a.Activity.Timestamp < start && a.Activity.Timestamp > end);

        List<IGrouping<ActivityGroup, ActivityEntity>> groups = await GroupActivities(userActivity).ToListAsync();

        DateTime oldestTime = groups.Max(g => g.Max(a => a.Timestamp));
        long oldestTimestamp = new DateTimeOffset(oldestTime).ToUnixTimeMilliseconds();

        return this.Ok(
            await GameStream.CreateFromEntityResult(this.database, token, groups, timestamp, oldestTimestamp));
    }
}