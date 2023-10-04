using System.Text.RegularExpressions;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Logging;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static partial class VersionHelper
{
    static VersionHelper()
    {
        try
        {
            CommitHash = ThisAssembly.Git.Commit;
            Branch = ThisAssembly.Git.Branch;
            string commitNumber = $"{CommitHash}_{Build}";
            FullRevision = Branch == "main" ? $"r{commitNumber}" : $"{Branch}_r{commitNumber}";
        }
        catch
        {
            Logger.Error("Project Lighthouse was built incorrectly. Please make sure git is available when building.",
                LogArea.Startup);
            CommitHash = "invalid";
            Branch = "invalid";
        }

        if (!IsDirty) return;

        Logger.Warn("This is a modified version of Project Lighthouse. " +
                    "Please make sure you are properly disclosing the source code to any users who may be using this instance.",
            LogArea.Startup);
    }

    public static string CommitHash { get; }
    public static string Branch { get; }
    /// <summary>
    ///     The full revision string. States current revision hash and, if not main, the branch.
    /// </summary>
    private static string FullRevision { get; set; }
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