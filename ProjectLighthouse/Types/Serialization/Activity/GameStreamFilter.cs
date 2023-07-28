#nullable enable
using System.Collections.Generic;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Activity;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity;

[XmlRoot("stream")]
// This class should only be deserialized
public class GameStreamFilter
{
    [XmlArray("sources")]
    [XmlArrayItem("source")]
    public List<GameStreamFilterEventSource>? Sources { get; set; }
}

[XmlRoot("source")]
public class GameStreamFilterEventSource
{
    [XmlAttribute("type")]
    public string? SourceType { get; set; }

    [XmlArray("event_filters")]
    [XmlArrayItem("event_filter")]
    public List<EventType>? Types { get; set; }
}