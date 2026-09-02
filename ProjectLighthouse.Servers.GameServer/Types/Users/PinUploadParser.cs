#nullable enable
using System.Text.Json;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Users;

public static class PinUploadParser
{
    public sealed class ParsedPinUpload
    {
        public Dictionary<uint, double> Progress { get; } = [];

        // Null means profile_pins was omitted
        public List<uint>? ProfilePins { get; internal set; }
    }

    public static bool TryParse(Pins upload, out ParsedPinUpload result)
    {
        result = new ParsedPinUpload();

        if (!TryParseProgress(upload.Progress, result))
            return false;

        if (!TryParseAwards(upload.Awards, result))
            return false;

        if (!TryParseProfilePins(upload.ProfilePins, result))
            return false;

        return true;
    }

    private static bool TryParseProgress(JsonElement[]? values, ParsedPinUpload result)
    {
        if (values == null)
            return true;

        if ((values.Length & 1) != 0)
            return false;

        for (int i = 0; i < values.Length; i += 2)
        {
            if (values[i].ValueKind != JsonValueKind.Number || !values[i].TryGetUInt32(out uint progressType))
            {
                return false;
            }

            if (values[i + 1].ValueKind != JsonValueKind.Number || !values[i + 1].TryGetDouble(out double value))
            {
                return false;
            }

            if (!double.IsFinite(value))
                return false;

            if (!result.Progress.TryAdd(progressType, value))
                return false;
        }

        return true;
    }

    private static bool TryParseAwards(JsonElement[]? values, ParsedPinUpload result)
    {
        if (values == null)
            return true;

        if ((values.Length & 1) != 0)
            return false;

        HashSet<uint> seenProgressTypes = [];

        for (int i = 0; i < values.Length; i += 2)
        {
            if (values[i].ValueKind != JsonValueKind.Number || !values[i].TryGetUInt32(out uint progressType))
            {
                return false;
            }

            if (values[i + 1].ValueKind != JsonValueKind.Number || !values[i + 1].TryGetInt64(out long count))
            {
                return false;
            }

            if (count < 0)
                return false;

            if (!seenProgressTypes.Add(progressType))
                return false;

            if (result.Progress.TryGetValue(progressType, out double existingValue))
            {
                result.Progress[progressType] = Math.Max(existingValue, count);
            }
            else
            {
                result.Progress.Add(progressType, count);
            }
        }

        return true;
    }

    private static bool TryParseProfilePins(JsonElement[]? values, ParsedPinUpload result)
    {
        if (values == null)
            return true;

        if (values.Length > 3)
            return false;

        List<uint> profilePins = new(values.Length);
        HashSet<uint> seenPinIds = [];

        foreach (JsonElement value in values)
        {
            if (value.ValueKind != JsonValueKind.Number || !value.TryGetUInt32(out uint pinId))
            {
                return false;
            }

            if (!seenPinIds.Add(pinId))
                return false;

            profilePins.Add(pinId);
        }

        result.ProfilePins = profilePins;

        return true;
    }
}
