using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.NewsPost"/>.
/// <remarks>
/// <para>
/// This event type can only be grouped with other <see cref="NewsActivityEntity"/>.
/// </para>
/// </remarks>
/// </summary>
public class NewsActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="WebsiteAnnouncementEntity.AnnouncementId"/> of the <see cref="WebsiteAnnouncementEntity"/> that this event refers to.
    /// </summary>
    public int NewsId { get; set; }

    [ForeignKey(nameof(NewsId))]
    public WebsiteAnnouncementEntity News { get; set; }
}