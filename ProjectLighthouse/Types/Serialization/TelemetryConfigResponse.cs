using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

//TODO what is the format for telemetry
[XmlRoot("t_enable")]
public class TelemetryConfigResponse : ILbpSerializable
{
    [XmlText]
    public bool TelemetryEnabled { get; set; }
    
}