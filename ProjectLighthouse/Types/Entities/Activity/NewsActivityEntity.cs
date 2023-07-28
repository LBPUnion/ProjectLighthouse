using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: NewsPost
/// </summary>
public class NewsActivityEntity : ActivityEntity
{
    public int NewsId { get; set; }

    [ForeignKey(nameof(NewsId))]
    public WebsiteAnnouncementEntity News { get; set; }
}