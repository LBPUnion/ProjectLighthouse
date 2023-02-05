using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

public enum SlotType
{
    [XmlEnum("developer")]
    Developer = 0,
    [XmlEnum("user")]
    User = 1,
    [XmlEnum("moon")]
    Moon = 2,
    Unknown = 3,
    Unknown2 = 4,
    [XmlEnum("pod")]
    Pod = 5,
    [XmlEnum("local")]
    Local = 6,
    DLC = 8,
}