using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("planetStats")]
public class PlanetStatsResponse : ILbpSerializable
{
    public PlanetStatsResponse() { }

    public PlanetStatsResponse(int totalSlots, int teamPicks)
    {
        this.TotalSlotCount = totalSlots;
        this.TeamPickCount = teamPicks;
    }

    [XmlElement("totalSlotCount")]
    public int TotalSlotCount { get; set; }

    [XmlElement("mmPicksCount")]
    public int TeamPickCount { get; set; }
}