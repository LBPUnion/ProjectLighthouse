using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types
{
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

        [XmlElement("score")]
        public int Points { get; set; }
    }
}