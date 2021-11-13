using System;
using System.Text.Json.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Match
{
    [Serializable]
    public class Player
    {
        public string PlayerId { get; set; }

        [JsonPropertyName("matching_res")]
        public int MatchingRes { get; set; }
    }
}