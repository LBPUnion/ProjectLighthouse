using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types
{
    [XmlRoot("photo")]
    [XmlType("photo")]
    public class Photo
    {
        [Key]
        public int PhotoId { get; set; }

        // Uses seconds instead of milliseconds for some reason
        [XmlAttribute("timestamp")]
        public long Timestamp { get; set; }

        [XmlElement("small")]
        public string SmallHash { get; set; }

        [XmlElement("medium")]
        public string MediumHash { get; set; }

        [XmlElement("large")]
        public string LargeHash { get; set; }

        [XmlElement("plan")]
        public string PlanHash { get; set; }

//        [XmlIgnore]
//        public int SlotId { get; set; }
//
//        [XmlIgnore]
//        [ForeignKey(nameof(SlotId))]
//        public Slot Slot { get; set; }

        /// <summary>
        /// Only use when parsing from XML.
        /// </summary>
        [NotMapped]
        [XmlArray("subjects")]
        [XmlArrayItem("subject")]
        public List<PhotoSubject> Subjects { get; set; }

        [NotMapped]
        [XmlIgnore]
        public string[] PhotoSubjectIds {
            get => this.PhotoSubjectCollection.Split(",");
            set => this.PhotoSubjectCollection = string.Join(',', value);
        }

        public string PhotoSubjectCollection { get; set; }
    }
}