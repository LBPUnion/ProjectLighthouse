using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests.Integration;

public sealed class DatabaseFactAttribute : FactAttribute
{
    private static readonly object migrateLock = new();

    public DatabaseFactAttribute()
    {
        ServerConfiguration.Instance.DbConnectionString = "server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse";
        if (!ServerStatics.DbConnected) this.Skip = "Database not available";
        else
            lock (migrateLock)
            {
                using DatabaseContext database = DatabaseContext.CreateNewInstance();
                database.Database.Migrate();
            }
    }
}