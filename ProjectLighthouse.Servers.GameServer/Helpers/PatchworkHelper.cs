using LBPUnion.ProjectLighthouse.Configuration;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public static class PatchworkHelper
{
    static int patchworkMajorVer = ServerConfiguration.Instance.Authentication.PatchworkMajorVersionMinimum;
    static int patchworkMinorVer = ServerConfiguration.Instance.Authentication.PatchworkMinorVersionMinimum;
    public static bool UserHasValidPatchworkUserAgent(string userAgent)
    {
        string userAgentPrefix = "PatchworkLBP";
        char gameVersion = userAgent[userAgentPrefix.Length];

        if (!userAgent.StartsWith(userAgentPrefix))
            return false;

        switch (gameVersion) {
            case '1':
            case '2':
            case '3':
            case 'V':
                break;
            default:
                return false;
        }

        string[] patchworkVer = userAgent.Split(' ')[1].Split('.');
        if (int.Parse(patchworkVer[0]) !>= patchworkMajorVer || int.Parse(patchworkVer[1]) !>= patchworkMinorVer)
            return false;

        return true;
    }
}