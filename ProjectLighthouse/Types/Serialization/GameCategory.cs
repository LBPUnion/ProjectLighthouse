using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("category")]
public class GameCategory : ILbpSerializable
{
    [XmlElement("name")]
    [DefaultValue("")]
    public string Name { get; set; }

    [XmlElement("description")]
    [DefaultValue("")]
    public string Description { get; set; }

    [XmlElement("url")]
    public string Url { get; set; }

    [XmlElement("icon")]
    [DefaultValue("")]
    public string Icon { get; set; }

    [DefaultValue("")]
    [XmlArray("sorts")]
    [XmlArrayItem("sort")]
    public string[] Sorts { get; set; }

    [DefaultValue("")]
    [XmlArray("types")]
    [XmlArrayItem("type")]
    public string[] Types { get; set; }

    [XmlElement("tag")]
    public string Tag { get; set; }

    [DefaultValue(null)]
    [XmlElement("results")]
    public GenericSerializableList? Results { get; set; }

    public static GameCategory CreateFromEntity(Category category, GenericSerializableList? results) =>
        new()
        {
            Name = category.Name,
            Description = category.Description,
            Icon = category.IconHash,
            Url = category.IngameEndpoint,
            Sorts = category.Sorts,
            Types = category.Types,
            Tag = category.Tag,
            Results = results,
        };
}