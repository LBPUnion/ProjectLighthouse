using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.News;

/// <summary>
/// Used in LBP2 and beyond
/// </summary>
[XmlRoot("item")]
public class GameNewsObject : ILbpSerializable
{
    [XmlElement("id")]
    public int Id { get; set; }

    [XmlElement("title")]
    public string Title { get; set; }

    [XmlElement("summary")]
    public string Summary { get; set; }

    [XmlElement("text")]
    public string Text { get; set; }

    [XmlElement("date")]
    public long Timestamp { get; set; }

    [XmlElement("image")]
    [DefaultValue(null)]
    public GameNewsImage Image { get; set; }

    [XmlElement("category")]
    public string Category { get; set; }

    public static GameNewsObject CreateFromEntity(WebsiteAnnouncementEntity entity) =>
        new()
        {
            Id = entity.AnnouncementId,
            Title = entity.Title,
            Summary = "there's an extra spot for summary here",
            Text = entity.Content,
            Category = "no_category",
        };
}

[XmlRoot("image")]
public class GameNewsImage : ILbpSerializable
{
    [XmlElement("hash")]
    public string Hash { get; set; }

    [XmlElement("alignment")]
    public string Alignment { get; set; }
}