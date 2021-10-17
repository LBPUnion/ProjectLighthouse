using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Types {
    /// <summary>
    /// A LittleBigPlanet level.
    /// </summary>
    [XmlRoot("slot"), XmlType("slot")]
    public class Slot {
        [XmlAttribute("type")]
        [NotMapped]
        public string Type { get; set; }

        [Key]
        [XmlIgnore]
        public int SlotId { get; set; }

        
        [XmlElement("name")]
        public string Name { get; set; }
        
        [XmlElement("description")]
        public string Description { get; set; }
        
        [XmlElement("icon")]
        public string IconHash { get; set; }
        
        [XmlElement("rootLevel")]
        public string RootLevel { get; set; }
        
        [XmlElement("resource")]
        public string Resource { get; set; }
        
        [XmlIgnore]
        public int LocationId { get; set; }

        [XmlIgnore] 
        public int CreatorId { get; set; }

        [ForeignKey(nameof(CreatorId))]
        public User Creator { get; set; }

        /// <summary>
        /// The location of the level on the creator's earth
        /// </summary>
        [XmlElement("location")]
        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; }
        
        [XmlElement("initiallyLocked")]
        public bool InitiallyLocked { get; set; }
        
        [XmlElement("isSubLevel")]
        public bool SubLevel { get; set; }
        
        [XmlElement("isLBP1Only")]
        public bool Lbp1Only { get; set; }
        
        [XmlElement("shareable")]
        public int Shareable { get; set; }
        
        [XmlElement("authorLabels")]
        public string AuthorLabels { get; set; }
        
        [XmlElement("background")]
        public string BackgroundHash { get; set; }
        
        [XmlElement("minPlayers")]
        public int MinimumPlayers { get; set; }
        
        [XmlElement("maxPlayers")]
        public int MaximumPlayers { get; set; }
        
        [XmlElement("moveRequired")]
        public bool MoveRequired { get; set; }

        public string Serialize() {
            string slotData = LbpSerializer.StringElement("name", Name) +
                              LbpSerializer.StringElement("id", SlotId) +
                              LbpSerializer.StringElement("game", 1) +
                              LbpSerializer.StringElement("npHandle", Creator.Username) +
                              LbpSerializer.StringElement("description", Description) +
                              LbpSerializer.StringElement("icon", IconHash) +
                              LbpSerializer.StringElement("rootLevel", RootLevel) +
                              LbpSerializer.StringElement("resource", Resource) +
                              LbpSerializer.StringElement("location", Location.Serialize()) +
                              LbpSerializer.StringElement("initiallyLocked", InitiallyLocked) +
                              LbpSerializer.StringElement("isSubLevel", SubLevel) +
                              LbpSerializer.StringElement("isLBP1Only", Lbp1Only) +
                              LbpSerializer.StringElement("shareable", Shareable) +
                              LbpSerializer.StringElement("background", BackgroundHash) +
                              LbpSerializer.StringElement("minPlayers", MinimumPlayers) +
                              LbpSerializer.StringElement("maxPlayers", MaximumPlayers) +
                              LbpSerializer.StringElement("moveRequired", MoveRequired);
            
            return LbpSerializer.TaggedStringElement("slot", slotData, "type", "user");
        }
    }
}