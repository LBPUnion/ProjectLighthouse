using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("npdata")]
public struct FriendResponse : ILbpSerializable
{

    public FriendResponse(List<MinimalUserProfile> friends)
    {
        this.Friends = friends;
    }

    [XmlArray("friends")]
    [XmlArrayItem("npHandle")]
    public List<MinimalUserProfile> Friends { get; set; }

}