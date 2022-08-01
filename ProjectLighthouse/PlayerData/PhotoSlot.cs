﻿using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Levels;

namespace LBPUnion.ProjectLighthouse.PlayerData;

[XmlRoot("slot")]
public class PhotoSlot
{
    [XmlAttribute("type")]
    public SlotType SlotType { get; set; }

    [XmlElement("id")]
    public int SlotId { get; set; }

    [XmlElement("rootLevel")]
    public string RootLevel { get; set; }

    [XmlElement("name")]
    public string LevelName { get; set; }
}