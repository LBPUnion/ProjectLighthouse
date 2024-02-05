using System.Collections.Generic;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("results")]
public struct GenericSerializableList : ILbpSerializable
{
    public GenericSerializableList(List<ILbpSerializable> items, int total, int hintStart)
    {
        this.Items = new SerializableList<ILbpSerializable>();
        this.Items.AddRange(items);
        this.Total = total;
        this.HintStart = hintStart;
    }

    public GenericSerializableList(List<ILbpSerializable> items, PaginationData pageData)
    {
        this.Items = new SerializableList<ILbpSerializable>();
        this.Items.AddRange(items);
        this.Total = pageData.TotalElements;
        this.HintStart = pageData.HintStart;
    }

    [XmlAnyElement]
    public SerializableList<ILbpSerializable> Items { get; set; }

    [XmlAttribute("total")]
    public int Total { get; set; }

    [XmlAttribute("hint_start")]
    public int HintStart { get; set; }
}