using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public class GameManualXml : ILbpSerializable
{
    [XmlText]
    public string Content { get; set; } = "";
}