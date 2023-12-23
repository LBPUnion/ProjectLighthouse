using System.Collections.Immutable;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Resources;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;

public static class GameResourceHelper
{
    private static readonly ImmutableList<string> textureGuids = ImmutableList<string>.Empty;

    static GameResourceHelper()
    {
        List<string> guids = new();
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

            guids.Add(guid);
        }
        textureGuids = ImmutableList.Create(guids.ToArray());
    }

    public static bool IsValidTexture(string resource)
    {
        if (!FileHelper.IsResourceValid(resource)) return false;

        if (resource.StartsWith("g"))
        {
            return textureGuids.Contains(resource[1..]);
        }

        return LbpFile.FromHash(resource)?.FileType is LbpFileType.Png or LbpFileType.Jpeg or LbpFileType.Plan
            or LbpFileType.Painting or LbpFileType.Texture;
    }
}