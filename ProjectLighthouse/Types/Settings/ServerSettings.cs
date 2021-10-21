#nullable enable
using System;

namespace ProjectLighthouse.Types.Settings {
    public static class ServerSettings {
        /// <summary>
        /// The maximum amount of slots allowed on users' earth
        /// </summary>
        public const int EntitledSlots = int.MaxValue;

        public const int ListsQuota = 20;

        public const string ServerName = "ProjectLighthouse";

        private static string? dbConnectionString;
        public static string DbConnectionString {
            get {
                if(dbConnectionString == null) {
                    return dbConnectionString = Environment.GetEnvironmentVariable("LIGHTHOUSE_DB_CONNECTION_STRING") ?? "";
                }
                return dbConnectionString;
            }
            set => dbConnectionString = value;
        }

        public static bool DbConnected {
            get {
                try {
                    return new Database().Database.CanConnect();
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                    return false;
                }
            }
        }
    }
}