using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.PlayerData;

[XmlRoot("playRecord")]
[XmlType("playRecord")]
public class Score
{
    [XmlIgnore]
    [Key]
    public int ScoreId { get; set; }

    [XmlIgnore]
    public int SlotId { get; set; }

    [XmlIgnore]
    [ForeignKey(nameof(SlotId))]
    public Slot Slot { get; set; }

    [XmlIgnore]
    public int ChildSlotId { get; set; }

    [XmlElement("type")]
    public int Type { get; set; }

    [XmlIgnore]
    public string PlayerIdCollection { get; set; }

    [NotMapped]
    [XmlElement("playerIds")]
    public string[] PlayerIds {
        get => this.PlayerIdCollection.Split(",");
        set => this.PlayerIdCollection = string.Join(',', value);
    }

    [NotMapped]
    [XmlElement("mainPlayer")]
    public string MainPlayer {
        get => this.PlayerIds[0];
        set => this.PlayerIds[0] = value;
    }

    [NotMapped]
    [XmlElement("rank")]
    public int Rank { get; set; }

    [XmlElement("score")]
    public int Points { get; set; }

    public string Serialize()
    {
        string score = LbpSerializer.StringElement("type", this.Type) +
                       LbpSerializer.StringElement("playerIds", this.PlayerIdCollection) +
                       LbpSerializer.StringElement("mainPlayer", this.MainPlayer) +
                       LbpSerializer.StringElement("rank", this.Rank) +
                       LbpSerializer.StringElement("score", this.Points);

        return LbpSerializer.StringElement("playRecord", score);
    }
}