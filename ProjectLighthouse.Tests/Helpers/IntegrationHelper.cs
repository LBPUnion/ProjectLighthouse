using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Tests.Helpers;

public static class IntegrationHelper
{
    private static readonly Lazy<bool> dbConnected = new(() =>
    {
        using DatabaseContext database = DatabaseContext.CreateNewInstance();
        return database.Database.CanConnect();
    });

    public static async Task<UserEntity> CreateRandomUser(string? password = null)
    {
        await using DatabaseContext database = DatabaseContext.CreateNewInstance();

        int userId = RandomNumberGenerator.GetInt32(int.MaxValue);
        const string username = "unitTestUser";
        // if user already exists, find another random number
        while (await database.Users.AnyAsync(u => u.Username == $"{username}{userId}"))
        {
            userId = RandomNumberGenerator.GetInt32(int.MaxValue);
        }

        UserEntity user = new()
        {
            UserId = userId,
            Username = $"{username}{userId}",
            Password = CryptoHelper.BCryptHash(CryptoHelper.Sha256Hash(password ?? $"unitTestPassword{userId}")),
            LinkedPsnId = (ulong)userId,
        };

        database.Add(user);
        await database.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Resets the database to a clean state and returns a new DatabaseContext.
    /// </summary>
    /// <returns>A new fresh instance of DatabaseContext</returns>
    public static async Task<DatabaseContext> GetIntegrationDatabase()
    {
        if (!dbConnected.Value)
        {
            throw new Exception("Database is not connected.\n" +
                                "Please ensure that the database is running and that the connection string is correct.\n" +
                                $"Connection string: {ServerConfiguration.Instance.DbConnectionString}");
        }
        await ClearRooms();
        await using DatabaseContext database = DatabaseContext.CreateNewInstance();
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        return DatabaseContext.CreateNewInstance();
    }

    private static async Task ClearRooms()
    {
        await RoomHelper.Rooms.RemoveAllAsync();
    }

}