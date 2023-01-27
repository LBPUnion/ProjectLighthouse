using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;

namespace LBPUnion.ProjectLighthouse.Administration.Reports;

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

[XmlRoot("griefReport")]
public class GriefReport
{
    [Key]
    public int ReportId { get; set; }

    [XmlElement("griefTypeId")]
    public GriefType Type { get; set; }

    public long Timestamp { get; set; }

    public int ReportingPlayerId { get; set; }

    [ForeignKey(nameof(ReportingPlayerId))]
    public User ReportingPlayer { get; set; }

    [NotMapped]
    [XmlElement("player")]
    public ReportPlayer[] XmlPlayers { get; set; }

    public string Players { get; set; }

    [XmlElement("griefStateHash")]
    public string GriefStateHash { get; set; }

    [XmlElement("levelOwner")]
    public string LevelOwner { get; set; }

    [XmlElement("initialStateHash")]
    public string InitialStateHash { get; set; }

    [XmlElement("jpegHash")]
    public string JpegHash { get; set; }

    [XmlElement("levelId")]
    public int LevelId { get; set; }

    [XmlElement("levelType")]
    public string LevelType { get; set; }

    [NotMapped]
    [XmlElement("marqee")]
    public Marqee XmlBounds { get; set; }

    public string Bounds { get; set; }

}