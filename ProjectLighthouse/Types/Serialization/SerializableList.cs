using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

/// <summary>
///     Shitty workaround to allow Lists of ILbpSerializable to be serialized.
/// </summary>
public class SerializableList<T> : List<T>, IXmlSerializable where T : ILbpSerializable
{
    public XmlSchema GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        foreach (CustomXmlSerializer xmlSerializer in this.Select(serializable => LighthouseSerializer.GetSerializer(serializable.GetType())))
        {
            xmlSerializer.Deserialize(reader);
        }
    }

    public void WriteXml(XmlWriter writer)
    {
        foreach (T serializable in this)
        {
            CustomXmlSerializer xmlSerializer = LighthouseSerializer.GetSerializer(serializable.GetType());
            xmlSerializer.Serialize(writer, serializable);
        }
    }
}