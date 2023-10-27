using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public class LbpCustomXml : ILbpSerializable
{
    public string Content { get; set; } = "";
}