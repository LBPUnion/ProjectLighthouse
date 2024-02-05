using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("users")]
public struct MinimalUserListResponse : ILbpSerializable
{
    public MinimalUserListResponse() { }

    public MinimalUserListResponse(List<MinimalUserProfile> users)
    {
        this.Users = users;
    }

    [XmlElement("user")]
    public List<MinimalUserProfile> Users { get; set; }
}

public class MinimalUserProfile : ILbpSerializable
{

    [XmlElement("npHandle")]
    public NpHandle UserHandle { get; set; } = new();
}