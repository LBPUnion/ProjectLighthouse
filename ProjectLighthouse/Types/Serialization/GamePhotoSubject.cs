using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlType("subject")]
[XmlRoot("subject")]
public class GamePhotoSubject : ILbpSerializable
{

    [XmlIgnore]
    public int UserId { get; set; }

    [XmlElement("npHandle")]
    public string Username { get; set; }

    [XmlElement("displayName")]
    public string DisplayName => this.Username;

    [XmlElement("bounds")]
    public string Bounds { get; set; }

    public static GamePhotoSubject CreateFromEntity(PhotoSubjectEntity entity) =>
        new()
        {
            UserId = entity.UserId,
            Bounds = entity.Bounds,
        };
}