using System.Collections.Generic;
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;

namespace LBPUnion.ProjectLighthouse.Extensions;

public static class ActivityQueryExtensions
{
    public static List<int> GetIds(this IReadOnlyCollection<OuterActivityGroup> groups, ActivityGroupType type)
    {
        List<int> ids = [];
        // Add outer group ids
        ids.AddRange(groups.Where(g => g.Key.GroupType == type)
            .Where(g => g.Key.TargetId != 0)
            .Select(g => g.Key.TargetId)
            .ToList());

        // Add specific event ids
        ids.AddRange(groups.SelectMany(g =>
            g.Groups.SelectMany(gr => gr.Where(a => a.GroupType == type).Select(a => a.TargetId))));
        if (type == ActivityGroupType.User)
        {
            ids.AddRange(groups.Where(g => g.Key.GroupType is not ActivityGroupType.News)
                .SelectMany(g => g.Groups.Select(a => a.Key.UserId)));
        }

        return ids.Distinct().ToList();
    }

    /// <summary>
    /// Turns a list of <see cref="ActivityDto"/> into a group based on its timestamp 
    /// </summary>
    /// <param name="activityQuery">An <see cref="IQueryable{ActivityDto}"/> to group</param>
    /// <param name="groupByActor">Whether or not the groups should be created based on the initiator of the event or the target of the event</param>
    /// <returns>The transformed query containing groups of <see cref="ActivityDto"/></returns>
    public static IQueryable<IGrouping<ActivityGroup, ActivityDto>> ToActivityGroups
        (this IQueryable<ActivityDto> activityQuery, bool groupByActor = false) =>
        groupByActor
            ? activityQuery.GroupBy(dto => new ActivityGroup
            {
                Timestamp = dto.Activity.Timestamp.Date,
                UserId = dto.Activity.UserId,
                TargetNewsId = dto.TargetNewsId ?? 0,
                TargetTeamPickSlotId = dto.TargetTeamPickId ?? 0,
            })
            : activityQuery.GroupBy(dto => new ActivityGroup
            {
                Timestamp = dto.Activity.Timestamp.Date,
                TargetUserId = dto.TargetUserId ?? 0,
                TargetSlotId = dto.TargetSlotId ?? 0,
                TargetPlaylistId = dto.TargetPlaylistId ?? 0,
                TargetNewsId = dto.TargetNewsId ?? 0,
            });

    public static List<OuterActivityGroup> ToOuterActivityGroups
        (this IEnumerable<IGrouping<ActivityGroup, ActivityDto>> activityGroups, bool groupByActor = false) =>
        // Pin news posts to the top
        activityGroups.OrderByDescending(g => g.Key.GroupType == ActivityGroupType.News ? 1 : 0)
            .ThenByDescending(g => g.MaxBy(a => a.Activity.Timestamp)?.Activity.Timestamp ?? g.Key.Timestamp)
            .Select(g => new OuterActivityGroup
            {
                Key = g.Key,
                Groups = g.OrderByDescending(a => a.Activity.Timestamp)
                    .GroupBy(gr => new InnerActivityGroup
                    {
                        Type = groupByActor
                            ? gr.GroupType
                            : gr.GroupType != ActivityGroupType.News
                                ? ActivityGroupType.User
                                : ActivityGroupType.News,
                        UserId = gr.Activity.UserId,
                        TargetId = groupByActor
                            ? gr.TargetId
                            : gr.GroupType != ActivityGroupType.News
                                ? gr.Activity.UserId
                                : gr.TargetNewsId ?? 0,
                    })
                    .ToList(),
            })
            .ToList();

    /// <summary>
    /// Converts an <see cref="IQueryable"/>&lt;<see cref="ActivityEntity"/>&gt; into an <see cref="IQueryable"/>&lt;<see cref="ActivityDto"/>&gt; for grouping.
    /// </summary>
    /// <param name="activityQuery">The activity query to be converted.</param>
    /// <param name="includeSlotCreator">Whether or not the <see cref="ActivityDto.TargetSlotCreatorId"/> field should be included.</param>
    /// <param name="includeTeamPick">Whether or not the <see cref="ActivityDto.TargetTeamPickId"/> field should be included.</param>
    /// <returns>The converted <see cref="IQueryable"/>&lt;<see cref="ActivityDto"/>&gt;</returns>
    public static IQueryable<ActivityDto> ToActivityDto
        (this IQueryable<ActivityEntity> activityQuery, bool includeSlotCreator = false, bool includeTeamPick = false)
    {
        return activityQuery.Select(a => new ActivityDto
        {
            Activity = a,
            TargetSlotId = (a as LevelActivityEntity).SlotId,
            TargetSlotGameVersion = (a as LevelActivityEntity).Slot.GameVersion,
            TargetSlotCreatorId = includeSlotCreator ? (a as LevelActivityEntity).Slot.CreatorId : null,
            TargetUserId = (a as UserActivityEntity).TargetUserId,
            TargetNewsId = (a as NewsActivityEntity).NewsId,
            TargetPlaylistId = (a as PlaylistActivityEntity).PlaylistId,
            TargetTeamPickId =
                includeTeamPick && a.Type == EventType.MMPickLevel ? (a as LevelActivityEntity).SlotId : null, });
    }

    /// <summary>
    /// Converts an IEnumerable&lt;<see cref="ActivityEntity"/>&gt; into an IEnumerable&lt;<see cref="ActivityDto"/>&gt; for grouping.
    /// </summary>
    /// <param name="activityEnumerable">The activity query to be converted.</param>
    /// <param name="includeSlotCreator">Whether or not the <see cref="ActivityDto.TargetSlotCreatorId"/> field should be included.</param>
    /// <param name="includeTeamPick">Whether or not the <see cref="ActivityDto.TargetTeamPickId"/> field should be included.</param>
    /// <returns>The converted IEnumerable&lt;<see cref="ActivityDto"/>&gt;</returns>
    public static IEnumerable<ActivityDto> ToActivityDto
        (this IEnumerable<ActivityEntity> activityEnumerable, bool includeSlotCreator = false, bool includeTeamPick = false)
    {
        return activityEnumerable.Select(a => new ActivityDto
        {
            Activity = a,
            TargetSlotId = (a as LevelActivityEntity)?.SlotId,
            TargetSlotGameVersion = (a as LevelActivityEntity)?.Slot.GameVersion,
            TargetSlotCreatorId = includeSlotCreator ? (a as LevelActivityEntity)?.Slot.CreatorId : null,
            TargetUserId = (a as UserActivityEntity)?.TargetUserId,
            TargetNewsId = (a as NewsActivityEntity)?.NewsId,
            TargetPlaylistId = (a as PlaylistActivityEntity)?.PlaylistId,
            TargetTeamPickId =
                includeTeamPick && a.Type == EventType.MMPickLevel ? (a as LevelActivityEntity)?.SlotId : null, });
    }
}