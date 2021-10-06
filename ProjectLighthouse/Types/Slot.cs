using System.ComponentModel.DataAnnotations.Schema;
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
        
        [XmlElement("location")]
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
        
    }
}