using LBPUnion.ProjectLighthouse.Configuration;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public static class PatchworkHelper
{
    static int patchworkMajorVer = ServerConfiguration.Instance.PatchworkMajorVersionMinimum;
    static int patchworkMinorVer = ServerConfiguration.Instance.PatchworkMinorVersionMinimum;
    public static bool UserHasValidPatchworkUserAgent(string userAgent)
    {
        string userAgentPrefix = "PatchworkLBP";
        char gameVersion = userAgent[userAgentPrefix.Length];

        if (userAgent.StartsWith(userAgentPrefix))
            return false;

        if (gameVersion is not '1' or '2' or '3' or 'V')
            return false;

        string[] patchworkVer = userAgent.Split(' ')[1].Split('.');
        if (int.Parse(patchworkVer[0]) >= patchworkMajorVer && int.Parse(patchworkVer[1]) >= patchworkMinorVer)
            return true;

        return false;
    }
}