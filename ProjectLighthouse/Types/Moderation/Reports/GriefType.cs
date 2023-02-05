using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Moderation.Reports;

public enum GriefType
{
    [XmlEnum("1")]
    Obscene = 1,

    [XmlEnum("2")]
    Mature = 2,

    [XmlEnum("3")]
    Offensive = 3,

    [XmlEnum("4")]
    Violence = 4,

    [XmlEnum("5")]
    Illegal = 5,

    [XmlEnum("6")]
    Unknown = 6,

    [XmlEnum("7")]
    Tos = 7,

    [XmlEnum("8")]
    Other = 8,
}