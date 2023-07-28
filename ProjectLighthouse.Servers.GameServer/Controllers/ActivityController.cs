using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter.Filters.Activity;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using LBPUnion.ProjectLighthouse.Types.Activity;
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

    /// <summary>
    /// This method is only used for LBP2 so we exclude playlists
    /// </summary>
    private async Task<IQueryable<ActivityDto>> GetFilters
    (
        IQueryable<ActivityDto> dtoQuery,
        GameTokenEntity token,
        bool excludeNews,
        bool excludeMyLevels,
        bool excludeFriends,
        bool excludeFavouriteUsers,
        bool excludeMyself,
        bool excludeMyPlaylists = true
    )
    {
        Expression<Func<ActivityDto, bool>> predicate = PredicateExtensions.False<ActivityDto>();

        List<int> favouriteUsers = await this.database.HeartedProfiles.Where(hp => hp.UserId == token.UserId)
            .Select(hp => hp.HeartedUserId)
            .ToListAsync();

        List<int>? friendIds = UserFriendStore.GetUserFriendData(token.UserId)?.FriendIds;
        friendIds ??= new List<int>();

        // This is how lbp3 does its filtering
        GameStreamFilter? filter = await this.DeserializeBody<GameStreamFilter>();
        if (filter?.Sources != null)
        {
            foreach (GameStreamFilterEventSource filterSource in filter.Sources.Where(filterSource =>
                         filterSource.SourceType != null && filterSource.Types?.Count != 0))
            {
                EventType[] types = filterSource.Types?.ToArray() ?? Array.Empty<EventType>();
                EventTypeFilter eventFilter = new(types);
                predicate = filterSource.SourceType switch
                {
                    "MyLevels" => predicate.Or(new MyLevelActivityFilter(token.UserId, eventFilter).GetPredicate()),
                    "FavouriteUsers" => predicate.Or(
                        new IncludeUserIdFilter(favouriteUsers, eventFilter).GetPredicate()),
                    "Friends" => predicate.Or(new IncludeUserIdFilter(friendIds, eventFilter).GetPredicate()),
                    _ => predicate,
                };
            }
        }

        Expression<Func<ActivityDto, bool>> newsPredicate = !excludeNews
            ? new IncludeNewsFilter().GetPredicate()
            : new ExcludeNewsFilter().GetPredicate();

        predicate = predicate.Or(newsPredicate);

        if (!excludeMyLevels)
        {
            predicate = predicate.Or(dto => dto.TargetSlotCreatorId == token.UserId);
        }

        List<int> includedUserIds = new();

        if (!excludeFriends)
        {
            includedUserIds.AddRange(friendIds);
        }

        if (!excludeFavouriteUsers)
        {
            includedUserIds.AddRange(favouriteUsers);
        }

        if (!excludeMyself)
        {
            includedUserIds.Add(token.UserId);
        }

        predicate = predicate.Or(dto => includedUserIds.Contains(dto.Activity.UserId));

        if (!excludeMyPlaylists)
        {
            List<int> creatorPlaylists = await this.database.Playlists.Where(p => p.CreatorId == token.UserId)
                .Select(p => p.PlaylistId)
                .ToListAsync();
            predicate = predicate.Or(new PlaylistActivityFilter(creatorPlaylists).GetPredicate());
        }
        else
        {
            predicate = predicate.And(dto =>
                dto.Activity.Type != EventType.CreatePlaylist &&
                dto.Activity.Type != EventType.HeartPlaylist &&
                dto.Activity.Type != EventType.AddLevelToPlaylist);
        }

        Console.WriteLine(predicate);

        dtoQuery = dtoQuery.Where(predicate);

        return dtoQuery;
    }

    public Task<DateTime> GetMostRecentEventTime(IQueryable<ActivityDto> activity, DateTime upperBound)
    {
        return activity.OrderByDescending(a => a.Activity.Timestamp)
            .Where(a => a.Activity.Timestamp < upperBound)
            .Select(a => a.Activity.Timestamp)
            .FirstOrDefaultAsync();
    }

    private async Task<(DateTime Start, DateTime End)> GetTimeBounds
        (IQueryable<ActivityDto> activityQuery, long? startTime, long? endTime)
    {
        if (startTime is null or 0) startTime = TimeHelper.TimestampMillis;

        DateTime start = DateTimeExtensions.FromUnixTimeMilliseconds(startTime.Value);
        DateTime end;

        if (endTime == null)
        {
            end = await this.GetMostRecentEventTime(activityQuery, start);
            // If there is no recent event then set it to the the start
            if (end == DateTime.MinValue) end = start;
            end = end.Subtract(TimeSpan.FromDays(7));
        }
        else
        {
            end = DateTimeExtensions.FromUnixTimeMilliseconds(endTime.Value);
            // Don't allow more than 7 days worth of activity in a single page
            if (start.Subtract(end).TotalDays > 7)
            {
                end = start.Subtract(TimeSpan.FromDays(7));
            }
        }

        return (start, end);
    }

    private static DateTime GetOldestTime
        (IReadOnlyCollection<IGrouping<ActivityGroup, ActivityDto>> groups, DateTime defaultTimestamp) =>
        groups.Any()
            ? groups.Min(g => g.MinBy(a => a.Activity.Timestamp)?.Activity.Timestamp ?? defaultTimestamp)
            : defaultTimestamp;

    /// <summary>
    /// Speeds up serialization because many nested entities need to find Slots by id
    /// and since they use the Find() method they can benefit from having the entities
    /// already tracked by the context
    /// </summary>
    private async Task CacheEntities(IReadOnlyCollection<OuterActivityGroup> groups)
    {
        List<int> slotIds = groups.GetIds(ActivityGroupType.Level);
        List<int> userIds = groups.GetIds(ActivityGroupType.User);
        List<int> playlistIds = groups.GetIds(ActivityGroupType.Playlist);
        List<int> newsIds = groups.GetIds(ActivityGroupType.News);

        // Cache target levels and users within DbContext
        if (slotIds.Count > 0) await this.database.Slots.Where(s => slotIds.Contains(s.SlotId)).LoadAsync();
        if (userIds.Count > 0) await this.database.Users.Where(u => userIds.Contains(u.UserId)).LoadAsync();
        if (playlistIds.Count > 0)
            await this.database.Playlists.Where(p => playlistIds.Contains(p.PlaylistId)).LoadAsync();
        if (newsIds.Count > 0)
            await this.database.WebsiteAnnouncements.Where(a => newsIds.Contains(a.AnnouncementId)).LoadAsync();
    }

    /// <summary>
    /// LBP3 uses a different grouping format that wants the actor to be the top level group and the events should be the subgroups
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> GlobalActivityLBP3
        (long timestamp, bool excludeMyPlaylists, bool excludeNews, bool excludeMyself)
    {
        GameTokenEntity token = this.GetToken();

        if (token.GameVersion != GameVersion.LittleBigPlanet3) return this.NotFound();

        IQueryable<ActivityDto> activityEvents = await this.GetFilters(
            this.database.Activities.ToActivityDto(true, true),
            token,
            excludeNews,
            true,
            true,
            true,
            excludeMyself,
            excludeMyPlaylists);

        (DateTime Start, DateTime End) times = await this.GetTimeBounds(activityEvents, timestamp, null);

        // LBP3 is grouped by actorThenObject meaning it wants all events by a user grouped together rather than
        // all user events for a level or profile grouped together
        List<IGrouping<ActivityGroup, ActivityDto>> groups = await activityEvents
            .Where(dto => dto.Activity.Timestamp < times.Start && dto.Activity.Timestamp > times.End)
            .ToActivityGroups(true)
            .ToListAsync();

        List<OuterActivityGroup> outerGroups = groups.ToOuterActivityGroups(true);

        long oldestTimestamp = GetOldestTime(groups, times.Start).ToUnixTimeMilliseconds();

        return this.Ok(GameStream.CreateFromGroups(token,
            outerGroups,
            times.Start.ToUnixTimeMilliseconds(),
            oldestTimestamp));
    }

    [HttpGet]
    public async Task<IActionResult> GlobalActivity
    (
        long timestamp,
        long endTimestamp,
        bool excludeNews,
        bool excludeMyLevels,
        bool excludeFriends,
        bool excludeFavouriteUsers,
        bool excludeMyself
    )
    {
        GameTokenEntity token = this.GetToken();

        if (token.GameVersion == GameVersion.LittleBigPlanet1) return this.NotFound();

        IQueryable<ActivityDto> activityEvents = await this.GetFilters(this.database.Activities.ToActivityDto(true),
            token,
            excludeNews,
            excludeMyLevels,
            excludeFriends,
            excludeFavouriteUsers,
            excludeMyself);

        (DateTime Start, DateTime End) times = await this.GetTimeBounds(activityEvents, timestamp, endTimestamp);

        List<IGrouping<ActivityGroup, ActivityDto>> groups = await activityEvents
            .Where(dto => dto.Activity.Timestamp < times.Start && dto.Activity.Timestamp > times.End)
            .ToActivityGroups()
            .ToListAsync();

        List<OuterActivityGroup> outerGroups = groups.ToOuterActivityGroups();

        long oldestTimestamp = GetOldestTime(groups, times.Start).ToUnixTimeMilliseconds();

        await this.CacheEntities(outerGroups);

        GameStream? gameStream = GameStream.CreateFromGroups(token,
            outerGroups,
            times.Start.ToUnixTimeMilliseconds(),
            oldestTimestamp);

        return this.Ok(gameStream);
    }

    #if DEBUG
    private static void PrintOuterGroups(List<OuterActivityGroup> outerGroups)
    {
        foreach (OuterActivityGroup outer in outerGroups)
        {
            Console.WriteLine(@$"Outer group key: {outer.Key}");
            List<IGrouping<InnerActivityGroup, ActivityDto>> itemGroup = outer.Groups;
            foreach (IGrouping<InnerActivityGroup, ActivityDto> item in itemGroup)
            {
                Console.WriteLine(
                    @$"  Inner group key: TargetId={item.Key.TargetId}, UserId={item.Key.UserId}, Type={item.Key.Type}");
                foreach (ActivityDto activity in item)
                {
                    Console.WriteLine(
                        @$"        Activity: {activity.GroupType}, Timestamp: {activity.Activity.Timestamp}, UserId: {activity.Activity.UserId}, EventType: {activity.Activity.Type}, TargetId: {activity.TargetId}");
                }
            }
        }
    }
    #endif

    [HttpGet("slot/{slotType}/{slotId:int}")]
    [HttpGet("user2/{username}")]
    public async Task<IActionResult> SlotActivity(string? slotType, int slotId, string? username, long? timestamp)
    {
        GameTokenEntity token = this.GetToken();

        if (token.GameVersion == GameVersion.LittleBigPlanet1) return this.NotFound();

        if ((SlotHelper.IsTypeInvalid(slotType) || slotId == 0) == (username == null)) return this.BadRequest();

        IQueryable<ActivityDto> activityQuery = this.database.Activities.ToActivityDto()
            .Where(a => a.Activity.Type != EventType.NewsPost && a.Activity.Type != EventType.MMPickLevel);

        bool isLevelActivity = username == null;

        // Slot activity
        if (isLevelActivity)
        {
            if (slotType == "developer")
                slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

            if (!await this.database.Slots.AnyAsync(s => s.SlotId == slotId)) return this.NotFound();

            activityQuery = activityQuery.Where(dto => dto.TargetSlotId == slotId);
        }
        // User activity
        else
        {
            int userId = await this.database.Users.Where(u => u.Username == username)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();
            if (userId == 0) return this.NotFound();
            activityQuery = activityQuery.Where(dto => dto.Activity.UserId == userId);
        }

        (DateTime Start, DateTime End) times = await this.GetTimeBounds(activityQuery, timestamp, null);

        activityQuery = activityQuery.Where(dto =>
            dto.Activity.Timestamp < times.Start && dto.Activity.Timestamp > times.End);

        List<IGrouping<ActivityGroup, ActivityDto>> groups = await activityQuery.ToActivityGroups().ToListAsync();

        List<OuterActivityGroup> outerGroups = groups.ToOuterActivityGroups();

        long oldestTimestamp = GetOldestTime(groups, times.Start).ToUnixTimeMilliseconds();

        await this.CacheEntities(outerGroups);

        return this.Ok(GameStream.CreateFromGroups(token,
            outerGroups,
            times.Start.ToUnixTimeMilliseconds(),
            oldestTimestamp));
    }
}