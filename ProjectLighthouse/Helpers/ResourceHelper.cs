using System;
using System.IO;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class ResourceHelper
{
    public static string ReadManifestFile(string fileName)
    {
        using Stream stream = typeof(Database).Assembly.GetManifestResourceStream($"{typeof(Database).Namespace}.{fileName}");
        using StreamReader reader = new(stream ?? throw new Exception("The assembly or manifest resource is null."));

        return reader.ReadToEnd().Trim();
    }
}