#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using LBPUnion.ProjectLighthouse.Types.Match;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class MatchHelper
    {
        public static readonly Dictionary<int, string?> UserLocations = new();
        public static readonly Dictionary<int, List<int>?> UserRecentlyDivedIn = new();

        public static void SetUserLocation(int userId, string location)
        {
            if (UserLocations.TryGetValue(userId, out string? _)) UserLocations.Remove(userId);
            UserLocations.Add(userId, location);
        }

        public static void AddUserRecentlyDivedIn(int userId, int otherUserId)
        {
            if (!UserRecentlyDivedIn.TryGetValue(userId, out List<int>? recentlyDivedIn)) UserRecentlyDivedIn.Add(userId, recentlyDivedIn = new List<int>());

            Debug.Assert(recentlyDivedIn != null, nameof(recentlyDivedIn) + " is null, somehow.");

            recentlyDivedIn.Add(otherUserId);
        }

        public static bool DidUserRecentlyDiveInWith(int userId, int otherUserId)
        {
            if (!UserRecentlyDivedIn.TryGetValue(userId, out List<int>? recentlyDivedIn) || recentlyDivedIn == null) return false;

            return recentlyDivedIn.Contains(otherUserId);
        }

        // This is the function used to show people how laughably awful LBP's protocol is. Beware.
        public static IMatchData? Deserialize(string data)
        {
            string matchType = "";

            int i = 1;
            while (true)
            {
                if (data[i] == ',') break;

                matchType += data[i];
                i++;
            }

            string matchData = $"{{{string.Concat(data.Skip(matchType.Length + 3).SkipLast(2))}}}"; // unfuck formatting so we can parse it as json

            // JSON does not like the hex value that location comes in (0x7f000001) so, convert it to int
            matchData = Regex.Replace(matchData, @"0x[a-fA-F0-9]{8}", m => Convert.ToInt32(m.Value, 16).ToString());
            // oh, but it gets better than that! LBP also likes to send hex values with an uneven amount of digits (0xa000064, 7 digits). in any case, we handle it here:
            matchData = Regex.Replace(matchData, @"0x[a-fA-F0-9]{7}", m => Convert.ToInt32(m.Value, 16).ToString());
            // i'm actually crying about it.

            return Deserialize(matchType, matchData);
        }

        public static IMatchData? Deserialize(string matchType, string matchData)
        {
            return matchType switch
            {
                "UpdateMyPlayerData" => JsonSerializer.Deserialize<UpdateMyPlayerData>(matchData),
                "UpdatePlayersInRoom" => JsonSerializer.Deserialize<UpdatePlayersInRoom>(matchData),
                "CreateRoom" => JsonSerializer.Deserialize<CreateRoom>(matchData),
                "FindBestRoom" => JsonSerializer.Deserialize<FindBestRoom>(matchData),
                _ => null,
            };
        }
    }
}