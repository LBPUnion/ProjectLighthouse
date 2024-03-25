using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.News;

/// <summary>
/// Used in LBP1 only
/// </summary>
[XmlRoot("news")]
public class GameNews : ILbpSerializable
{
    [XmlElement("subcategory")]
    public List<GameNewsSubcategory> Entries { get; set; }

    public static GameNews CreateFromEntity(List<WebsiteAnnouncementEntity> entities) =>
        new()
        {
            Entries = entities.Select(entity => new GameNewsSubcategory
                {
                    Item = new GameNewsItem
                    {
                        Content = new GameNewsContent
                        {
                            Frame = new GameNewsFrame
                            {
                                Title = entity.Title,
                                Width = 512,
                                Container = new List<GameNewsFrameContainer>
                                {
                                    new()
                                    {
                                        Content = entity.Content,
                                        Width = 512,
                                    },
                                },
                            },
                        },
                    },
                })
                .ToList(),
        };
}

[XmlRoot("subcategory")]
public class GameNewsSubcategory : ILbpSerializable
{
    [XmlElement("item")]
    public GameNewsItem Item { get; set; }
}

public class GameNewsItem : ILbpSerializable
{
    [XmlElement("content")]
    public GameNewsContent Content { get; set; }
}

public class GameNewsContent : ILbpSerializable
{
    [XmlElement("frame")]
    public GameNewsFrame Frame { get; set; }
}