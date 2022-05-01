using System;
using System.IO;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class ResourceHelper
{
    public static string readManifestFile(string fileName)
    {
        using Stream stream = typeof(Program).Assembly.GetManifestResourceStream($"{typeof(Program).Namespace}.{fileName}");
        using StreamReader reader = new(stream ?? throw new Exception("The assembly or manifest resource is null."));

        return reader.ReadToEnd().Trim();
    }
}