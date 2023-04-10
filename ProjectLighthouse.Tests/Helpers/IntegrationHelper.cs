using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;

namespace LBPUnion.ProjectLighthouse.Tests.Helpers;

public static class IntegrationHelper
{

    private static readonly Lazy<bool> dbConnected = new(ServerStatics.DbConnected);

    /// <summary>
    /// Resets the database to a clean state and returns a new DatabaseContext.
    /// </summary>
    /// <returns>A new fresh instance of DatabaseContext</returns>
    public static async Task<DatabaseContext> GetIntegrationDatabase()
    {
        if (!dbConnected.Value)
        {
            throw new Exception($"Database is not connected.\n" +
                                $"Please ensure that the database is running and that the connection string is correct.\n" +
                                $"Connection string: {ServerConfiguration.Instance.DbConnectionString}");
        }
        await using DatabaseContext database = new();
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        return new DatabaseContext();
    } 

}