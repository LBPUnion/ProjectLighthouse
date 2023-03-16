using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Moderation.Reports;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Moderation;

public class GriefReportEntity
{
    [Key]
    public int ReportId { get; set; }

    public GriefType Type { get; set; }

    public long Timestamp { get; set; }

    public int ReportingPlayerId { get; set; }

    [ForeignKey(nameof(ReportingPlayerId))]
    public UserEntity ReportingPlayer { get; set; }

    public string Players { get; set; }

    public string GriefStateHash { get; set; }

    public string LevelOwner { get; set; }

    public string InitialStateHash { get; set; }

    public string JpegHash { get; set; }

    public int LevelId { get; set; }

    public string LevelType { get; set; }

    public string Bounds { get; set; }

    [NotMapped]
    public Marqee XmlBounds { get; set; }

    [NotMapped]
    public ReportPlayer[] XmlPlayers { get; set; }

}