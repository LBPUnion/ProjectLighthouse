using LBPUnion.ProjectLighthouse.Configuration;
using System.Text.RegularExpressions;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public static class PatchworkHelper
{
    static int requiredMajor = ServerConfiguration.Instance.Authentication.PatchworkMajorVersionMinimum;
    static int requiredMinor = ServerConfiguration.Instance.Authentication.PatchworkMinorVersionMinimum;
    public static bool IsValidPatchworkUserAgent(string userAgent)
    {
        Match result = Regex.Match(userAgent, @"^PatchworkLBP[123V] (\d{1,5})\.(\d{1,5})$");
        if (!result.Success) return false;

        if (!int.TryParse(result.Groups[1].Value, out int major) || !int.TryParse(result.Groups[2].Value, out int minor))
            return false;

        if (major < requiredMajor || minor < requiredMinor)
            return false;

        return true;
    }
}