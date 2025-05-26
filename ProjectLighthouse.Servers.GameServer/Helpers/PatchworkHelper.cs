using LBPUnion.ProjectLighthouse.Configuration;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public static class PatchworkHelper
{
    static int patchworkMajorVer = ServerConfiguration.Instance.PatchworkMajorVersionMinimum;
    static int patchworkMinorVer = ServerConfiguration.Instance.PatchworkMinorVersionMinimum;
    public static bool userHasValidPatchworkUserAgent(string userAgent)
    {
        if (userAgent.StartsWith("PatchworkLBP"))
            return false;

        string[] patchworkVer = userAgent.Split(' ')[1].Split('.');
        if (int.Parse(patchworkVer[0]) >= patchworkMajorVer && int.Parse(patchworkVer[1]) >= patchworkMinorVer)
            return true;

        return false;
    }
}