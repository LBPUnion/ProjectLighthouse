using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Moderation.Reports;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("griefReport")]
public class GameGriefReport : ILbpSerializable
{
    [XmlElement("griefTypeId")]
    public GriefType Type { get; set; }

    [XmlElement("player")]
    public ReportPlayer[] XmlPlayers { get; set; }

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

    [XmlElement("marqee")]
    public Marqee XmlBounds { get; set; }

    public static GriefReportEntity ConvertToEntity(GameGriefReport report) =>
        new()
        {
            Type = report.Type,
            LevelOwner = report.LevelOwner,
            LevelId = report.LevelId,
            LevelType = report.LevelType,
            JpegHash = report.JpegHash,
            InitialStateHash = report.InitialStateHash,
        };

}