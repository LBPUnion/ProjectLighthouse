#nullable enable
using System;
using System.Linq;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Types.Settings
{
    public static class ServerStatics
    {
        public const string ServerName = "ProjectLighthouse";

        public static bool DbConnected {
            get {
                try
                {
                    return new Database().Database.CanConnect();
                }
                catch(Exception e)
                {
                    Logger.Log(e.ToString(), LoggerLevelDatabase.Instance);
                    return false;
                }
            }
        }

        public static bool IsUnitTesting => AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.FullName.StartsWith("xunit"));

        public const int PageSize = 20;
    }
}