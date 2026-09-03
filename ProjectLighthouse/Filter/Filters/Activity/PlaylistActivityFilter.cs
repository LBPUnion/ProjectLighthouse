using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Filter.Filters.Activity;

public class PlaylistActivityFilter : IActivityFilter
{
    private readonly List<int> playlistIds;
    private readonly EventTypeFilter eventFilter;

    public PlaylistActivityFilter(List<int> playlistIds, EventTypeFilter eventFilter = null)
    {
        this.playlistIds = playlistIds;
        this.eventFilter = eventFilter;
    }

    public Expression<Func<ActivityDto, bool>> GetPredicate()
    {
        Expression<Func<ActivityDto, bool>> predicate = PredicateExtensions.False<ActivityDto>();
        predicate = this.playlistIds.Aggregate(predicate, (current, playlistId) => current.Or(a => (a.Activity is PlaylistActivityEntity || a.Activity is PlaylistWithSlotActivityEntity) && a.TargetPlaylistId == playlistId));
        if (this.eventFilter != null) predicate = predicate.And(this.eventFilter.GetPredicate());
        return predicate;
    }
}