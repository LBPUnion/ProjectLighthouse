using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types
{
//    [XmlRoot("subject")]
    [XmlType("subject")]
    [Serializable]
    public class PhotoSubject
    {
        [Key]
        [XmlIgnore]
        public int PhotoSubjectId { get; set; }

        [XmlIgnore]
        public int UserId { get; set; }

        [XmlIgnore]
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [NotMapped]
        [XmlElement("npHandle")]
        public string Username { get; set; }

        [XmlElement("bounds")]
        public string Bounds { get; set; }
    }
}