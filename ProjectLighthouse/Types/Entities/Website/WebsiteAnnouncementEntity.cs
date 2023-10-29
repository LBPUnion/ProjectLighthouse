using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Website;

public class WebsiteAnnouncementEntity
{
    [Key]
    public int AnnouncementId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    #nullable enable

    public int? PublisherId { get; set; }

    [ForeignKey(nameof(PublisherId))]
    public UserEntity? Publisher { get; set; }
}