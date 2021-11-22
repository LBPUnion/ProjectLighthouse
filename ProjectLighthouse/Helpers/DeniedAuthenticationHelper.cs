using System.Collections.Generic;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class DeniedAuthenticationHelper
    {
        public static readonly Dictionary<string, long> IPAddressAndNameDeniedAt = new();

        public static void Set(string ipAddressAndName, long timestamp = 0)
        {
            if (timestamp == 0) timestamp = TimestampHelper.Timestamp;

            if (IPAddressAndNameDeniedAt.TryGetValue(ipAddressAndName, out long _)) IPAddressAndNameDeniedAt.Remove(ipAddressAndName);
            IPAddressAndNameDeniedAt.Add(ipAddressAndName, timestamp);
        }

        public static bool RecentlyDenied(string ipAddressAndName)
        {
            if (!IPAddressAndNameDeniedAt.TryGetValue(ipAddressAndName, out long timestamp)) return false;

            return TimestampHelper.Timestamp < timestamp + 60;
        }
    }
}