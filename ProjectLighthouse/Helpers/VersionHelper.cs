using System;
using System.IO;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Settings;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class VersionHelper
    {
        static VersionHelper()
        {
            try
            {
                CommitHash = readManifestFile("gitVersion.txt");
                Branch = readManifestFile("gitBranch.txt");
                CanCheckForUpdates = true;
            }
            catch
            {
                Logger.Log
                (
                    "Project Lighthouse was built incorrectly. Please make sure git is available when building. " +
                    "Because of this, you will not be notified of updates.",
                    LoggerLevelStartup.Instance
                );
                CommitHash = "invalid";
                Branch = "invalid";
                CanCheckForUpdates = false;
            }

            if (IsDirty)
            {
                Logger.Log
                (
                    "This is a modified version of Project Lighthouse. " +
                    "Please make sure you are properly disclosing the source code to any users who may be using this instance.",
                    LoggerLevelStartup.Instance
                );
                CanCheckForUpdates = false;
            }
        }

        private static string readManifestFile(string fileName)
        {
            using Stream stream = typeof(Program).Assembly.GetManifestResourceStream($"{typeof(Program).Namespace}.{fileName}");
            using StreamReader reader = new(stream ?? throw new Exception("The assembly or manifest resource is null."));

            return reader.ReadToEnd().Trim();
        }

        public static string CommitHash { get; set; }
        public static string Branch { get; set; }
        public static string FullVersion => $"{ServerStatics.ServerName} {Branch}@{CommitHash} {Build}";
        public static bool IsDirty => CommitHash.EndsWith("-dirty");
        public static bool CanCheckForUpdates { get; set; }

        public const string Build =
            #if DEBUG
            "Debug";
        #elif RELEASE
            "Release";
        #else
            "Unknown";
        #endif
    }
}