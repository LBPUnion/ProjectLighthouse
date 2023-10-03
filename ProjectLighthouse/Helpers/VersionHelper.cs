using System.Linq;
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
            CommitHash = ResourceHelper.ReadManifestFile("gitVersion.txt");
            Branch = ResourceHelper.ReadManifestFile("gitBranch.txt");
            string commitNumber = $"{CommitHash}_{Build}";
            FullRevision = Branch == "main" ? $"r{commitNumber}" : $"{Branch}_r{commitNumber}";

            string remotesFile = ResourceHelper.ReadManifestFile("gitRemotes.txt");

            string[] lines = remotesFile.Split('\n');

            // line[0] line[1]                                        line[2]
            // origin  git@github.com:LBPUnion/project-lighthouse.git (fetch)

            // linq is a serious and painful catastrophe but its useful so i'm gonna keep using it
            Remotes = lines.Select(line => line.Split("\t")[1]).ToArray();

            CommitsOutOfDate = ResourceHelper.ReadManifestFile("gitUnpushed.txt").Split('\n').Length;

            CanCheckForUpdates = true;
        }
        catch
        {
            Logger.Error("Project Lighthouse was built incorrectly. Please make sure git is available when building.",
                LogArea.Startup);
            CommitHash = "invalid";
            Branch = "invalid";
            CanCheckForUpdates = false;
        }

        if (!IsDirty) return;

        Logger.Warn("This is a modified version of Project Lighthouse. " +
                    "Please make sure you are properly disclosing the source code to any users who may be using this instance.",
            LogArea.Startup);
        CanCheckForUpdates = false;
    }

    [GeneratedRegex(@"((git|ssh|http(s)?)|(git@[\w\.-]+))(:(\/\/)?)([\w\.@\:\/\-~]+)((\.git)(\/))?( .+)?")]
    private static partial Regex GitRemoteRegex();

    #nullable enable
    /// <summary>
    ///     Determines the URL of the git remote.
    /// </summary>
    public static string? DetermineRemoteUrl()
    {
        string? remote = Remotes?.FirstOrDefault();
        if (remote == null) return null;

        Match match = GitRemoteRegex().Match(remote);

        if (!match.Success || match.Groups.Count != 12) return null;

        return match.Groups[7].Value;
    }
    #nullable disable

    public static string CommitHash { get; set; }
    public static string Branch { get; set; }
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
    public static bool IsDirty => CommitHash.EndsWith("-dirty") ||
                                  CommitsOutOfDate != 1 ||
                                  CommitHash == "invalid" ||
                                  Branch == "invalid";
    public static int CommitsOutOfDate { get; set; }
    public static bool CanCheckForUpdates { get; set; }
    public static string[] Remotes { get; set; }

    public const string Build =
#if DEBUG
        "Debug";
#elif RELEASE
        "Release";
#else
        "Unknown";
#endif
}