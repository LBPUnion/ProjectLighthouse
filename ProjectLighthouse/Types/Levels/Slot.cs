using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Profiles;

namespace LBPUnion.ProjectLighthouse.Types.Levels
{
    /// <summary>
    ///     A LittleBigPlanet level.
    /// </summary>
    [XmlRoot("slot")]
    [XmlType("slot")]
    public class Slot
    {
        [XmlAttribute("type")]
        [NotMapped]
        public string Type { get; set; }

        [Key]
        [XmlElement("id")]
        public int SlotId { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("icon")]
        public string IconHash { get; set; }

        [XmlElement("rootLevel")]
        public string RootLevel { get; set; }

        public string ResourceCollection { get; set; }

        [NotMapped]
        [XmlElement("resource")]
        public string[] Resources {
            get => this.ResourceCollection.Split(",");
            set => this.ResourceCollection = string.Join(',', value);
        }

        [XmlIgnore]
        public int LocationId { get; set; }

        [XmlIgnore]
        public int CreatorId { get; set; }

        [ForeignKey(nameof(CreatorId))]
        public User Creator { get; set; }

        /// <summary>
        ///     The location of the level on the creator's earth
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
        public string BackgroundHash { get; set; } = "";

        [XmlElement("minPlayers")]
        public int MinimumPlayers { get; set; }

        [XmlElement("maxPlayers")]
        public int MaximumPlayers { get; set; }

        [XmlElement("moveRequired")]
        public bool MoveRequired { get; set; }

        [XmlIgnore]
        public long FirstUploaded { get; set; }

        [XmlIgnore]
        public long LastUpdated { get; set; }

        [XmlIgnore]
        public bool MMPick { get; set; }

        public string SerializeResources()
        {
            return this.Resources.Aggregate("", (current, resource) => current + LbpSerializer.StringElement("resource", resource));
        }

        public string Serialize()
        {
            string slotData = LbpSerializer.StringElement("name", this.Name) +
                              LbpSerializer.StringElement("id", this.SlotId) +
                              LbpSerializer.StringElement("game", 1) +
                              LbpSerializer.StringElement("npHandle", this.Creator.Username) +
                              LbpSerializer.StringElement("description", this.Description) +
                              LbpSerializer.StringElement("icon", this.IconHash) +
                              LbpSerializer.StringElement("rootLevel", this.RootLevel) +
                              this.SerializeResources() +
                              LbpSerializer.StringElement("location", this.Location.Serialize()) +
                              LbpSerializer.StringElement("initiallyLocked", this.InitiallyLocked) +
                              LbpSerializer.StringElement("isSubLevel", this.SubLevel) +
                              LbpSerializer.StringElement("isLBP1Only", this.Lbp1Only) +
                              LbpSerializer.StringElement("shareable", this.Shareable) +
                              LbpSerializer.StringElement("background", this.BackgroundHash) +
                              LbpSerializer.StringElement("minPlayers", this.MinimumPlayers) +
                              LbpSerializer.StringElement("maxPlayers", this.MaximumPlayers) +
                              LbpSerializer.StringElement("moveRequired", this.MoveRequired) +
                              LbpSerializer.StringElement("firstPublished", this.FirstUploaded) +
                              LbpSerializer.StringElement("lastUpdated", this.LastUpdated) +
                              LbpSerializer.StringElement("mmpick", this.MMPick);

            return LbpSerializer.TaggedStringElement("slot", slotData, "type", "user");
        }
    }
}