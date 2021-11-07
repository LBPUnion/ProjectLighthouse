using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

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

        public string Serialize()
        {
            string response = LbpSerializer.StringElement("npHandle", User.Username) +
                              LbpSerializer.StringElement("displayName", User.Username) +
                              LbpSerializer.StringElement("bounds", Bounds);

            return LbpSerializer.StringElement("subject", response);
        }
    }
}