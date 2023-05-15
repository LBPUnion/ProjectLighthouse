using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public struct GenericSlotResponse : ILbpSerializable, IHasCustomRoot
{
    public GenericSlotResponse() { }

    public GenericSlotResponse(string rootElement, List<SlotBase> slots, int total = 0, int hintStart = 0)
    {
        this.RootTag = rootElement;
        this.Slots = slots;
        this.Total = total;
        this.HintStart = hintStart;
    }

    public GenericSlotResponse(string rootElement, List<SlotBase> slots, PaginationData pageData)
    {
        this.RootTag = rootElement;
        this.Slots = slots;
        this.Total = pageData.TotalElements;
        this.HintStart = pageData.HintStart;
    }

    public GenericSlotResponse(List<SlotBase> slots, PaginationData pageData) : this("slots", slots, pageData) { }

    public GenericSlotResponse(List<SlotBase> slots) : this("slots", slots) { }

    public GenericSlotResponse(List<SlotBase> slots, int total, int hintStart) : this("slots", slots, total, hintStart) { }

    [XmlIgnore]
    private string RootTag { get; }

    [XmlElement("slot")]
    public List<SlotBase> Slots { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("total")]
    public int Total { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("hint_start")]
    public int HintStart { get; set; }

    public string GetRoot() => this.RootTag;
}