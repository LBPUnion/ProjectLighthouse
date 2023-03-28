using System.Xml;

namespace LBPUnion.ProjectLighthouse.Serialization;

public class XmlWriterWrapper : XmlWriter
{
    private readonly XmlWriter writer;

    protected XmlWriterWrapper(XmlWriter baseWriter)
    {
        this.writer = baseWriter;
    }

    public override void Flush()
    {
        this.writer.Flush();
    }

    public override string LookupPrefix(string ns) => this.writer.LookupPrefix(ns);

    public override void WriteBase64(byte[] buffer, int index, int count)
    {
        this.writer.WriteBase64(buffer, index, count);
    }

    public override void WriteCData(string text)
    {
        this.writer.WriteCData(text);
    }

    public override void WriteCharEntity(char ch)
    {
        this.writer.WriteCharEntity(ch);
    }

    public override void WriteChars(char[] buffer, int index, int count)
    {
        this.writer.WriteChars(buffer, index, count);
    }

    public override void WriteComment(string text)
    {
        this.writer.WriteComment(text);
    }

    public override void WriteDocType(string name, string pubid, string sysid, string subset)
    {
        this.writer.WriteDocType(name, pubid, sysid, subset);
    }

    public override void WriteEndAttribute()
    {
        this.writer.WriteEndAttribute();
    }

    public override void WriteEndDocument()
    {
        this.writer.WriteEndDocument();
    }

    public override void WriteEndElement()
    {
        this.writer.WriteEndElement();
    }

    public override void WriteEntityRef(string name)
    {
        this.writer.WriteEntityRef(name);
    }

    public override void WriteFullEndElement()
    {
        this.writer.WriteFullEndElement();
    }

    public override void WriteProcessingInstruction(string name, string text)
    {
        this.writer.WriteProcessingInstruction(name, text);
    }

    public override void WriteRaw(char[] buffer, int index, int count)
    {
        this.writer.WriteRaw(buffer, index, count);
    }

    public override void WriteRaw(string data)
    {
        this.writer.WriteRaw(data);
    }

    public override void WriteStartAttribute(string prefix, string localName, string ns)
    {
        this.writer.WriteStartAttribute(prefix, localName, ns);
    }

    public override void WriteStartDocument()
    {
        this.writer.WriteStartDocument();
    }

    public override void WriteStartDocument(bool standalone)
    {
        this.writer.WriteStartDocument(standalone);
    }

    public override void WriteStartElement(string prefix, string localName, string ns)
    {
        this.writer.WriteStartElement(prefix, localName, ns);
    }

    public override void WriteString(string text)
    {
        this.writer.WriteString(text);
    }

    public override void WriteSurrogateCharEntity(char lowChar, char highChar)
    {
        this.writer.WriteSurrogateCharEntity(lowChar, highChar);
    }

    public override void WriteWhitespace(string ws)
    {
        this.writer.WriteWhitespace(ws);
    }

    public override WriteState WriteState => this.writer.WriteState;
}