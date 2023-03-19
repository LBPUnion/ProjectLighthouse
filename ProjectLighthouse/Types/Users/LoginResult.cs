using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Users;

/// <summary>
///     Response to POST /login
/// </summary>
[XmlRoot("loginResult")]
[XmlType("loginResult")]
public class LoginResult : ILbpSerializable
{
    [XmlElement("authTicket")]
    public string AuthTicket { get; set; }

    [XmlElement("lbpEnvVer")]
    public string ServerBrand { get; set; }

    [XmlElement("titleStorageURL")]
    public string TitleStorageUrl { get; set; }
}