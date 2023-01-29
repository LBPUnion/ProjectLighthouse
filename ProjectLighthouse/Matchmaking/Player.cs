using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using LBPUnion.ProjectLighthouse.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Matchmaking;

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