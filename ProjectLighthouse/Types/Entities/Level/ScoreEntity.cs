using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Level;

public class ScoreEntity
{
    [Key]
    public int ScoreId { get; set; }

    public int SlotId { get; set; }

    [XmlIgnore]
    [ForeignKey(nameof(SlotId))]
    public SlotEntity Slot { get; set; }

    [XmlIgnore]
    public int ChildSlotId { get; set; }

    public int Type { get; set; }

    public string PlayerIdCollection { get; set; }

    [NotMapped]
    public string[] PlayerIds {
        get => this.PlayerIdCollection.Split(",");
        set => this.PlayerIdCollection = string.Join(',', value);
    }

    public int Points { get; set; }
}