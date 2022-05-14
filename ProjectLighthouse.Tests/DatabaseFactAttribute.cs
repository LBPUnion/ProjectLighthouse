using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LBPUnion.ProjectLighthouse.Tests;

public sealed class DatabaseFactAttribute : FactAttribute
{
    private static readonly object migrateLock = new();

    public DatabaseFactAttribute()
    {
        ServerConfiguration.Instance = new ServerConfiguration();
        ServerConfiguration.Instance.DbConnectionString = "server=127.0.0.1;uid=root;pwd=lighthouse;database=lighthouse";
        if (!ServerStatics.DbConnected) this.Skip = "Database not available";
        else
            lock(migrateLock)
            {
                using Database database = new();
                database.Database.Migrate();
            }
    }
}