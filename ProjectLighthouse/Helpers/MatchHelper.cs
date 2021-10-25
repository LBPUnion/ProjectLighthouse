#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using LBPUnion.ProjectLighthouse.Types.Match;

namespace LBPUnion.ProjectLighthouse.Helpers {
    public static class MatchHelper {
        public static IMatchData? Deserialize(string data) {
            string matchType = "";

            int i = 1;
            while(true) {
                if(data[i] == ',') break;

                matchType += data[i];
                i++;
            }

            string matchData = $"{{{string.Concat(data.Skip(matchType.Length + 3).SkipLast(2))}}}";

            return Deserialize(matchType, matchData);
        }

        public static IMatchData? Deserialize(string matchType, string matchData) {
            return matchType switch {
                "UpdateMyPlayerData" => JsonSerializer.Deserialize<UpdateMyPlayerData>(matchData),
                "UpdatePlayersInRoom" => JsonSerializer.Deserialize<UpdatePlayersInRoom>(matchData),
                _ => null,
            };
        }
    }
}