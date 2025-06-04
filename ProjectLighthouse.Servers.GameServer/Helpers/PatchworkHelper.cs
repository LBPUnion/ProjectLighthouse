using LBPUnion.ProjectLighthouse.Configuration;
using System.Text.RegularExpressions;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public static partial class PatchworkHelper
{
    private static readonly int requiredMajor = ServerConfiguration.Instance.Authentication.PatchworkMajorVersionMinimum;
    private static readonly int requiredMinor = ServerConfiguration.Instance.Authentication.PatchworkMinorVersionMinimum;

    [GeneratedRegex(@"^PatchworkLBP[123V] (\d{1,5})\.(\d{1,5})$")]
    private static partial Regex PatchworkUserAgentRegex();

    public static bool IsValidPatchworkUserAgent(string userAgent)
    {
        Match result = PatchworkUserAgentRegex().Match(userAgent);
        if (!result.Success) return false;

        if (!int.TryParse(result.Groups[1].Value, out int major) || !int.TryParse(result.Groups[2].Value, out int minor))
            return false;

        return major >= requiredMajor && minor >= requiredMinor;
    }
}