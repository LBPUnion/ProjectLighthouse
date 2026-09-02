#nullable enable
using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("category")]
public class GameCategory : ILbpSerializable
{
    [XmlElement("name")]
    [DefaultValue("")]
    public string Name { get; set; } = string.Empty;

    [XmlElement("description")]
    [DefaultValue("")]
    public string Description { get; set; } = string.Empty;

    [XmlElement("url")]
    public string Url { get; set; } = string.Empty;

    [XmlElement("tag")]
    public string Tag { get; set; } = string.Empty;

    [XmlElement("icon")]
    [DefaultValue("")]
    public string Icon { get; set; } = string.Empty;

    [XmlElement("curated")]
    public bool Curated { get; set; }

    [XmlElement("disableFilters")]
    public bool DisableFilters { get; set; }

    [DefaultValue("")]
    [XmlArray("sorts")]
    [XmlArrayItem("sort")]
    public string[] Sorts { get; set; } = [];

    [DefaultValue("")]
    [XmlArray("types")]
    [XmlArrayItem("type")]
    public string[] Types { get; set; } = [];

    // This will likely be used in the future if Companion Capers ever get added in LBP3
    [DefaultValue("")]
    [XmlElement("param")]
    public string? Param { get; set; }

    [DefaultValue(null)]
    [XmlElement("defaultFilters")]
    public CategoryDefaults? DefaultFilters { get; set; }

    [DefaultValue(null)]
    [XmlElement("results")]
    public GenericSerializableList? Results { get; set; }

    public static GameCategory CreateFromEntity(Category category, GenericSerializableList? results) => new()
    {
        Name = category.Name,
        Description = category.Description,
        Icon = category.IconHash,
        Url = category.IngameEndpoint,
        Sorts = category.Sorts,
        Types = category.Types,
        Tag = category.Tag,
        Curated = category.Curated,
        DisableFilters = category.DisableFilters,
        Param = category.Param,
        DefaultFilters = category.DefaultFilters,
        Results = results,
    };
}
