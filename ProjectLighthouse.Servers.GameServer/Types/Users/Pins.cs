#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Users;

public class Pins
{
    [JsonPropertyName("progress")]
    public JsonElement[]? Progress { get; set; }

    [JsonPropertyName("awards")]
    public JsonElement[]? Awards { get; set; }

    [JsonPropertyName("profile_pins")]
    public JsonElement[]? ProfilePins { get; set; }
}
