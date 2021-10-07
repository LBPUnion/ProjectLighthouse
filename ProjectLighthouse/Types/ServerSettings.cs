#nullable enable
using System;

namespace ProjectLighthouse.Types {
    public static class ServerSettings {
        /// <summary>
        /// The maximum amount of slots allowed on users' earth
        /// </summary>
        public const int EntitledSlots = 20;

        public const int ListsQuota = 20;

        private static string? dbConnectionString;
        public static string DbConnectionString {
            get {
                if(dbConnectionString == null) {
                    return dbConnectionString = Environment.GetEnvironmentVariable("LIGHTHOUSE_DB_CONNECTION_STRING") ?? "";
                };
                return dbConnectionString;
            }
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