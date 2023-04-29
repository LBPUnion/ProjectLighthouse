using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("photos")]
public struct PhotoListResponse : ILbpSerializable
{
    public PhotoListResponse(List<GamePhoto> photos)
    {
        this.Photos = photos;
    }

    [XmlElement("photo")]
    public List<GamePhoto> Photos { get; set; }
    
}