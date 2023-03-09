using System.IO;
using System.Xml;

namespace LBPUnion.ProjectLighthouse.Serialization;

public class WriteFullClosingTagXmlWriter : ExcludeNamespaceXmlWriter
{
    public WriteFullClosingTagXmlWriter(TextWriter stringWriter, XmlWriterSettings settings) : base(stringWriter, settings) { }

    public override void WriteEndElement()
    {
        base.WriteFullEndElement();
    }
}