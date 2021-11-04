using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types
{
    [XmlRoot("subject")]
    [XmlType("subject")]
    public class PhotoSubject
    {
        [Key]
        public int PhotoSubjectId { get; set; }

        [XmlIgnore]
        public int UserId;

        [XmlIgnore]
        [ForeignKey(nameof(UserId))]
        public User User;

        [NotMapped]
        [XmlElement("npHandle")]
        public string Username;

        [XmlElement("bounds")]
        public string Bounds;
    }
}