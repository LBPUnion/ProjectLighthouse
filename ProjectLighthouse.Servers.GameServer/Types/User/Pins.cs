using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types;

public class Pins
{
    [JsonPropertyName("progress")]
    public long[]? Progress { get; set; }

    [JsonPropertyName("awards")]
    public long[]? Awards { get; set; }

    [JsonPropertyName("profile_pins")]
    public long[]? ProfilePins { get; set; }
}