using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("category")]
public class GameCategory : ILbpSerializable
{
    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("description")]
    public string Description { get; set; }

    [XmlElement("url")]
    public string Url { get; set; }

    [XmlElement("icon")]
    public string Icon { get; set; }

    [XmlElement("results")]
    public GenericSlotResponse Results { get; set; }

    public static GameCategory CreateFromEntity(Category category, GenericSlotResponse results) =>
        new()
        {
            Name = category.Name,
            Description = category.Description,
            Icon = category.IconHash,
            Url = category.IngameEndpoint,
            Results = results,
        };



}