using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Types;
using Xunit;

namespace ProjectLighthouse.Tests {
    public sealed class DatabaseFact : FactAttribute {
        public DatabaseFact() {
            ServerSettings.DbConnectionString = "server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse";
            if(!ServerSettings.DbConnected) Skip = "Database not available";
            else {
                using Database database = new();
                database.Database.Migrate();
            }
        }
    }
}