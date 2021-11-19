#nullable enable
using System;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Types.Settings
{
    public static class ServerStatics
    {
        /// <summary>
        ///     The maximum amount of slots allowed on users' earth
        /// </summary>
        public const int EntitledSlots = 50;

        public const int ListsQuota = 50;

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
    }
}