using System.Linq;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class VersionHelper
{
    static VersionHelper()
    {
        string commitNumber = "invalid";
        try
        {
            CommitHash = ResourceHelper.ReadManifestFile("gitVersion.txt");
            Branch = ResourceHelper.ReadManifestFile("gitBranch.txt");
            bool isShallowRepo = ResourceHelper.ReadManifestFile("gitIsShallowRepo.txt") == "true";
            if (isShallowRepo)
            {
                Logger.Warn
                (
                    "The UseLessReliavleNumericRevisionNumberingSystem option is not supported for builds made from a shallow clone." +
                    "Please perform a full clone if you want to use numeric revision numbers." +
                    "UseLessReliavleNumericRevisionNumberingSystem is now disabled.",
                    LogArea.Startup
                );
                ServerConfiguration.Instance.Customization.UseLessReliableNumericRevisionNumberingSystem = false;
            }
            commitNumber = ServerConfiguration.Instance.Customization.UseLessReliableNumericRevisionNumberingSystem ? ResourceHelper.ReadManifestFile("gitRevCount.txt") : $"{CommitHash}_{Build}";
            OrdinalCommitNumber = (Branch == "main") ? $"r{commitNumber}" : $"{Branch}_r{commitNumber}";

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
            Logger.Error
            (
                "Project Lighthouse was built incorrectly. Please make sure git is available when building.",
//                "Because of this, you will not be notified of updates.",
                LogArea.Startup
            );
            CommitHash = "invalid";
            Branch = "invalid";
            CanCheckForUpdates = false;
        }

        if (IsDirty)
        {
            Logger.Warn
            (
                "This is a modified version of Project Lighthouse. " +
                "Please make sure you are properly disclosing the source code to any users who may be using this instance.",
                LogArea.Startup
            );
            if (ServerConfiguration.Instance.Customization.UseLessReliableNumericRevisionNumberingSystem) // remove redundancy
                OrdinalCommitNumber = $"{Branch}-dirty_r{commitNumber}";
            CanCheckForUpdates = false;
        }
    }

    public static string CommitHash { get; set; }
    public static string Branch { get; set; }
    /// <summary>
    /// The amount of commits that currently exist in the local copy of the Git repository.
    /// Not reliable as versioning if shallow cloning is used, with using the hash of the current commit being more reliable in that case.
    /// </summary>
    public static string OrdinalCommitNumber { get; set; }
    /// <summary>
    /// The server's branding (environment version) to show to LBP clients. Shows the environment name next to the revision.
    /// </summary>
    public static string EnvVer => $"{ServerConfiguration.Instance.Customization.EnvironmentName} {OrdinalCommitNumber}";
    public static string FullVersion => $"Project Lighthouse {ServerConfiguration.Instance.Customization.EnvironmentName} {Branch}@{CommitHash} {Build}";
    public static bool IsDirty => CommitHash.EndsWith("-dirty") || CommitsOutOfDate != 1 || CommitHash == "invalid" || Branch == "invalid";
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