using System.Collections.Generic;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class DeniedAuthenticationHelper
{
    public static readonly Dictionary<string, long> IPAddressAndNameDeniedAt = new();
    public static readonly Dictionary<string, int> AttemptsByIPAddressAndName = new();

    public static void SetDeniedAt(string ipAddressAndName, long timestamp = 0)
    {
        if (timestamp == 0) timestamp = TimestampHelper.Timestamp;

        if (IPAddressAndNameDeniedAt.TryGetValue(ipAddressAndName, out long _)) IPAddressAndNameDeniedAt.Remove(ipAddressAndName);
        IPAddressAndNameDeniedAt.Add(ipAddressAndName, timestamp);
    }

    public static bool RecentlyDenied(string ipAddressAndName)
    {
        if (!IPAddressAndNameDeniedAt.TryGetValue(ipAddressAndName, out long timestamp)) return false;

        return TimestampHelper.Timestamp < timestamp + 300;
    }

    public static void AddAttempt(string ipAddressAndName)
    {
        if (AttemptsByIPAddressAndName.TryGetValue(ipAddressAndName, out int attempts)) AttemptsByIPAddressAndName.Remove(ipAddressAndName);
        AttemptsByIPAddressAndName.Add(ipAddressAndName, attempts + 1);
    }

    public static int GetAttempts(string ipAddressAndName)
    {
        if (!AttemptsByIPAddressAndName.TryGetValue(ipAddressAndName, out int attempts)) return 0;

        return attempts;
    }
}