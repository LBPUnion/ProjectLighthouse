using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Profile;

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
    [JsonIgnore]
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    [XmlIgnore]
    public int PhotoId { get; set; }

    [XmlIgnore]
    [JsonIgnore]
    [ForeignKey(nameof(PhotoId))]
    public Photo Photo { get; set; }

    [NotMapped]
    [XmlElement("npHandle")]
    public string Username { get; set; }

    [XmlElement("bounds")]
    public string Bounds { get; set; }

    public string Serialize()
    {
        string response = LbpSerializer.StringElement("npHandle", this.User.Username) +
                          LbpSerializer.StringElement("displayName", this.User.Username) +
                          LbpSerializer.StringElement("bounds", this.Bounds);

        return LbpSerializer.StringElement("subject", response);
    }
}