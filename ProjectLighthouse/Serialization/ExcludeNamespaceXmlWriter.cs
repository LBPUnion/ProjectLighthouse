using System.IO;
using System.Xml;

namespace LBPUnion.ProjectLighthouse.Serialization;

/// <summary>
/// Used to prevent the XmlWriter from writing out namespaces for abstract types
/// <para>
/// Example: When serializing a SlotBase an extra 'p2:type="UserSlot' and 'xmlns:p2' is added but 
/// with this workaround, these extra attributes will no longer be serialized
/// </para> 
/// </summary>
public class ExcludeNamespaceXmlWriter : XmlWriterWrapper
{
    public ExcludeNamespaceXmlWriter(TextWriter stringWriter, XmlWriterSettings settings) : base(Create(stringWriter, settings)) { }

    private bool skipAttribute;

    public override void WriteEndAttribute()
    {
        // Once we reach the end of the attribute then stop skipping attributes.
        if (this.skipAttribute)
        {
            this.skipAttribute = false;
            return;
        }
        base.WriteEndAttribute();
    }

    // This workaround is only for serializing abstract classes, which should always get serialized as a string.
    // Therefore it shouldn't be necessary to overwrite every WriteX method
    public override void WriteString(string text)
    {
        if (this.skipAttribute)
        {
            return;
        }
        base.WriteString(text);
    }

    // Ignores namespaces (xmlns attributes)
    public override void WriteStartElement(string prefix, string localName, string ns)
    {
        base.WriteStartElement(prefix, localName, "");
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
        // If serializer tries to write a namespace, then skip the next attribute
        if (ns != "" && prefix != "")
        {
            this.skipAttribute = true;
            return;
        }
        base.WriteStartAttribute(prefix, localName, ns);
    }
}