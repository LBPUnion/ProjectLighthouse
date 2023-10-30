using LBPUnion.ProjectLighthouse.Configuration;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class VersionHelper
{
    public static string CommitHash => ThisAssembly.Git.Commit;
    public static string Branch => ThisAssembly.Git.Branch;

    /// <summary>
    ///     The full revision string. States current revision hash and, if not main, the branch.
    /// </summary>
    private static string FullRevision => (Branch == "main" ? "" : $"{Branch}_") + $"r{CommitHash}_{Build}";

    /// <summary>
    ///     The server's branding (environment version) to show to LBP clients. Shows the environment name next to the
    ///     revision.
    /// </summary>
    public static string EnvVer => $"{ServerConfiguration.Instance.Customization.EnvironmentName} {FullRevision}";
    public static string FullVersion =>
        $"Project Lighthouse {ServerConfiguration.Instance.Customization.EnvironmentName} {Branch}@{CommitHash} {Build}";
    public static bool IsDirty => ThisAssembly.Git.IsDirty;
    public static string RepositoryUrl => ThisAssembly.Git.RepositoryUrl;

    public const string Build =
    #if DEBUG
        "Debug";
    #elif RELEASE
        "Release";
    #else
         "Unknown";
    #endif
}