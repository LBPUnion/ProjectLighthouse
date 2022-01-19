using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types;

[XmlRoot("deleted_by")]
public enum DeletedBy
{
    [XmlEnum(Name = "none")]
    None,

    [XmlEnum(Name = "moderator")]
    Moderator,

    [XmlEnum(Name = "level_author")]
    LevelAuthor,
    // TODO: deletion types for comments (profile etc) 
}