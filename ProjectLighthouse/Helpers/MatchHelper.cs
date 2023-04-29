#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using LBPUnion.ProjectLighthouse.Types.Matchmaking.MatchCommands;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static partial class MatchHelper
{
    public static readonly ConcurrentDictionary<int, string?> UserLocations = new();
    public static readonly ConcurrentDictionary<int, List<int>?> UserRecentlyDivedIn = new();

    public static void SetUserLocation(int userId, string location)
    {
        if (UserLocations.TryGetValue(userId, out string? _)) UserLocations.TryRemove(userId, out _);
        UserLocations.TryAdd(userId, location);
    }

    public static void AddUserRecentlyDivedIn(int userId, int otherUserId)
    {
        if (!UserRecentlyDivedIn.TryGetValue(userId, out List<int>? recentlyDivedIn)) UserRecentlyDivedIn.TryAdd(userId, recentlyDivedIn = new List<int>());

        Debug.Assert(recentlyDivedIn != null, nameof(recentlyDivedIn) + " is null, somehow.");

        recentlyDivedIn.Add(otherUserId);
    }

    public static bool DidUserRecentlyDiveInWith(int userId, int otherUserId)
    {
        if (!UserRecentlyDivedIn.TryGetValue(userId, out List<int>? recentlyDivedIn) || recentlyDivedIn == null) return false;

        return recentlyDivedIn.Contains(otherUserId);
    }

    public static void ClearUserRecentDiveIns(int userId) => UserRecentlyDivedIn.TryRemove(userId, out _);

    [GeneratedRegex("^\\[([^,]*),\\[(.*)\\]\\]")]
    private static partial Regex MatchJsonRegex();

    [GeneratedRegex(@"0x[a-fA-F0-9]{7,8}")]
    private static partial Regex LocationHexRegex();

    // This is the function used to show people how laughably awful LBP's protocol is. Beware.
    public static IMatchCommand? Deserialize(string data)
    {
        Match match = MatchJsonRegex().Match(data);

        if (!match.Success) return null;

        string matchType = match.Groups[1].Value;
        // Wraps the actual match data in curly braces to parse it as a json object
        string matchData = $"{{{match.Groups[2].Value}}}";

        // JSON does not like the hex value that location comes in (0x7f000001) so, convert it to int
        matchData = LocationHexRegex().Replace(matchData, m => Convert.ToInt32(m.Value, 16).ToString());

        return Deserialize(matchType, matchData);
    }

    private static IMatchCommand? Deserialize(string matchType, string matchData)
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