using System.ComponentModel.DataAnnotations;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Website;

public class WebsiteAnnouncementEntity
{
    [Key]
    public int AnnouncementId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }
}