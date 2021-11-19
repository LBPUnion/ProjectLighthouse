using System;
using System.IO;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class GitVersionHelper
    {
        static GitVersionHelper()
        {
            try
            {
                CommitHash = readManifestFile("gitVersion.txt");
                Branch = readManifestFile("gitBranch.txt");
                CanCheckForUpdates = true;
            }
            catch
            {
                Logger.Log("Project Lighthouse was built incorrectly. Please make sure git is available when building.", LoggerLevelStartup.Instance);
                CommitHash = "invalid";
                Branch = "invalid";
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
        public static bool CanCheckForUpdates { get; set; }
    }
}