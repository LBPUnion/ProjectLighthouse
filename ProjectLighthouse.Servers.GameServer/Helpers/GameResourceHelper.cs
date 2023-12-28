using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Resources;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public static class GameResourceHelper
{
    private static readonly HashSet<string> textureGuids = new();

    static GameResourceHelper()
    {
        using Stream? guidStream = typeof(GameResourceHelper).Assembly.GetManifestResourceStream("LBPUnion.ProjectLighthouse.Servers.GameServer.textureGuids.txt");
        if (guidStream == null)
        {
            Logger.Warn("Failed to load texture guids, users may experience issues when setting level and profile icons", LogArea.Startup);
            return;
        }

        using StreamReader reader = new(guidStream);
        while (!reader.EndOfStream)
        {
            string? guid = reader.ReadLine();
            if (guid == null) continue;

            textureGuids.Add(guid);
        }
    }

    public static bool IsValidTexture(string resource)
    {
        if (!FileHelper.IsResourceValid(resource)) return false;

        if (resource.StartsWith("g")) return textureGuids.Contains(resource[1..]);

        return LbpFile.FromHash(resource)?.FileType is LbpFileType.Png or LbpFileType.Jpeg or LbpFileType.Painting
            or LbpFileType.Texture;
    }

    public static bool IsValidLevel(string resource)
    {
        if (!FileHelper.IsResourceValid(resource)) return false;

        // Technically this method could be used (and is used) to check if a planet is valid,
        // but I'm keeping the method name as is for semantic reasons.
        return LbpFile.FromHash(resource)?.FileType is LbpFileType.Level;
    }
}