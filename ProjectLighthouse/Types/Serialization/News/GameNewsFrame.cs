#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Serialization.Slot;
using LBPUnion.ProjectLighthouse.Types.Serialization.User;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.News;

[XmlRoot("frame")]
public class GameNewsFrame : ILbpSerializable
{
    [XmlAttribute("width")]
    public int Width { get; set; }

    [XmlElement("title")]
    public string Title { get; set; } = "";

    [XmlElement("item")]
    [DefaultValue(null)]
    public List<GameNewsFrameContainer>? Container { get; set; }
}

public class GameNewsFrameContainer : ILbpSerializable
{
    [XmlAttribute("width")]
    public int Width { get; set; }

    [XmlElement("content")]
    [DefaultValue(null)]
    public string Content { get; set; } = "";

    [XmlElement("npHandle")]
    [DefaultValue(null)]
    public MinimalUserProfile? User { get; set; }

    [XmlElement("slot")]
    [DefaultValue(null)]
    public MinimalSlot? Slot { get; set; }
}