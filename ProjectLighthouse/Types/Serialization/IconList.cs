using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public struct IconList : ILbpSerializable
{
    public IconList() { }

    public IconList(List<string> iconHashList)
    {
        this.IconHashList = iconHashList;
    }

    [XmlElement("icon")]
    public List<string> IconHashList { get; set; }
}