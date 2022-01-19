using System.Collections.Generic;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types;

/// <summary>
///     Response to POST /login
/// </summary>
[XmlRoot("loginResult")]
[XmlType("loginResult")]
public class LoginResult
{
    [XmlElement("authTicket")]
    public string AuthTicket { get; set; }

    [XmlElement("lbpEnvVer")]
    public string LbpEnvVer { get; set; }

    public string Serialize()
        => LbpSerializer.Elements
            (new KeyValuePair<string, object>("authTicket", this.AuthTicket), new KeyValuePair<string, object>("lbpEnvVer", this.LbpEnvVer));
}