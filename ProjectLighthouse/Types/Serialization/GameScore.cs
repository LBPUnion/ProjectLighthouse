using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("playRecord")]
[XmlType("playRecord")]
public class GameScore : ILbpSerializable
{
    [XmlElement("type")]
    public int Type { get; set; }

    [XmlElement("playerIds")]
    public string[] PlayerIds;

    [DefaultValue("")]
    [XmlElement("mainPlayer")]
    public string MainPlayer { get; set; }

    [DefaultValue(0)]
    [XmlElement("rank")]
    public int Rank { get; set; }

    [XmlElement("score")]
    public int Points { get; set; }

    public static GameScore CreateFromEntity(ScoreEntity entity, int rank) =>
        new()
        {
            MainPlayer = entity.PlayerIds.ElementAtOrDefault(0) ?? "",
            PlayerIds = entity.PlayerIds,
            Points = entity.Points,
            Type = entity.Type,
            Rank = rank,
        };

}