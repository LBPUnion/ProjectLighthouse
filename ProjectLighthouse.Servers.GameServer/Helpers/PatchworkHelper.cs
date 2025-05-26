using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public static class PatchworkHelper
{
    static int patchworkMajorVer = ServerConfiguration.Instance.Authentication.PatchworkMajorVersionMinimum;
    static int patchworkMinorVer = ServerConfiguration.Instance.Authentication.PatchworkMinorVersionMinimum;
    public static bool UserHasValidPatchworkUserAgent(string userAgent)
    {
        string userAgentPrefix = "PatchworkLBP";
        char gameVersion = userAgent[userAgentPrefix.Length];
        int numericVersion = 0;

        if (userAgent.StartsWith(userAgentPrefix))
            return false;

        if (char.IsLetterOrDigit(gameVersion))
        {
            if (gameVersion == 'V')
                numericVersion = 4;
        }
        else
            numericVersion = gameVersion - '0';

        // Don't want it to be 0 still because of Unknown (-1) in GameVersion
        if (numericVersion - 1 == 0 || !Enum.IsDefined(typeof(GameVersion), numericVersion))
            return false;

        string[] patchworkVer = userAgent.Split(' ')[1].Split('.');
        if (int.Parse(patchworkVer[0]) >= patchworkMajorVer && int.Parse(patchworkVer[1]) >= patchworkMinorVer)
            return true;

        return false;
    }
}