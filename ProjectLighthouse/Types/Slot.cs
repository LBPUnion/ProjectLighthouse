using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Serialization;

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

        private Location location;

        /// <summary>
        /// The location of the level on the creator's earth
        /// </summary>
        [XmlElement("location")]
        public Location Location {
            get {
                if(location != null) return this.location;

                return location = new Database().Locations.First(l => l.Id == LocationId);
            }
        }
        
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
    }
}