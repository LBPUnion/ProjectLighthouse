using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Profiles;

[XmlRoot("npdata")]
[XmlType("npdata")]
public class NPData
{
    [XmlArray("friends")]
    [XmlArrayItem("npHandle")]
    public List<string> Friends { get; set; }

    [XmlArray("blocked")]
    [XmlArrayItem("npHandle")]
    public List<string> BlockedUsers { get; set; }
}