#nullable enable
using System;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;

namespace LBPUnion.ProjectLighthouse.Types.Settings
{
    public static class ServerSettings
    {
        /// <summary>
        ///     The maximum amount of slots allowed on users' earth
        /// </summary>
        public const int EntitledSlots = 50;

        public const int ListsQuota = 50;

        public const string ServerName = "ProjectLighthouse";

        private static string? dbConnectionString;

        public static string DbConnectionString {
            get {
                if (dbConnectionString == null) return dbConnectionString = Environment.GetEnvironmentVariable("LIGHTHOUSE_DB_CONNECTION_STRING") ?? "";

                return dbConnectionString;
            }
            set => dbConnectionString = value;
        }

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