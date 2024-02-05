using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("comments")]
public struct CommentListResponse : ILbpSerializable
{
    public CommentListResponse(List<GameComment> comments)
    {
        this.Comments = comments;
    }

    [XmlElement("comment")]
    public List<GameComment> Comments { get; set; }
}