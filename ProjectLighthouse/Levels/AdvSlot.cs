#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Reviews;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Levels;

/*  
    Heavily modified Slot.cs
    A large number of this is unused in Adventure slots, as the slots themselves 
    do not track plays, reviews, etc, only the scoreboard, author tags, title, description, could be more 
    but I don't currently have LBP3 PS4 running to check

    LittleBigPlanet 3 Adventure Sub-Slot.
*/
[XmlRoot("slot")]
[XmlType("advslot")]
public class AdvSlot
{
    [NotMapped]
    [JsonIgnore]
    [XmlIgnore]
    private Database? _database;

    [NotMapped]
    [JsonIgnore]
    [XmlIgnore]
    private Database database {
        get {
            if (this._database != null) return this._database;

            return this._database = new Database();
        }
        set => this._database = value;
    }

    [XmlAttribute("type")]
    [JsonIgnore]
    public SlotType Type { get; set; } = SlotType.User;

    [Key]
    [XmlElement("id")]
    public int SlotId { get; set; }

    [XmlElement("name")]
    public string Name { get; set; } = "";

    [XmlElement("description")]
    public string Description { get; set; } = "";

    [XmlElement("minPlayers")]
    public int MinimumPlayers { get; set; }

    [XmlElement("maxPlayers")]
    public int MaximumPlayers { get; set; }

    [XmlElement("moveRequired")]
    public bool MoveRequired { get; set; }

    [XmlElement("leveltype")]
    public string LevelType { get; set; } = "";

    public string Serialize
    (
        GameVersion gameVersion = GameVersion.LittleBigPlanet3,
        RatedLevel? yourRatingStats = null,
        VisitedLevel? yourVisitedStats = null,
        Review? yourReview = null,
        bool fullSerialization = false
    )
    {
        string slotData = LbpSerializer.StringElement("id", this.SlotId) +
                          LbpSerializer.StringElement("name", this.Name) +
                          LbpSerializer.StringElement("description", this.Description) +
                          LbpSerializer.StringElement("leveltype", this.LevelType) +
                          LbpSerializer.StringElement("minPlayers", this.MinimumPlayers) +
                          LbpSerializer.StringElement("maxPlayers", this.MaximumPlayers) +
                          (fullSerialization ? LbpSerializer.StringElement("moveRequired", this.MoveRequired) : "");
        return LbpSerializer.TaggedStringElement("slot", slotData, "type", "user");
    }
}