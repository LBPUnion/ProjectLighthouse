using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Match;

[Serializable]
public class Player
{
    [JsonIgnore]
    public User User { get; set; }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public string PlayerId => this.User.Username;

    [JsonPropertyName("matching_res")]
    public int MatchingRes { get; set; }
}