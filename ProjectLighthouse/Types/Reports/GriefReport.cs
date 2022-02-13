using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Reports;

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