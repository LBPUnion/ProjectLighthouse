using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests
{
    public sealed class DatabaseFact : FactAttribute
    {
        public DatabaseFact()
        {
            ServerStatics.DbConnectionString = "server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse";
            if (!ServerStatics.DbConnected)
            {
                this.Skip = "Database not available";
            }
            else
            {
                using Database database = new();
                database.Database.Migrate();
            }
        }
    }
}