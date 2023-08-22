using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class MigrationHelper
{
    public static async Task<List<T>> GetAllObjects<T>(DbContext database, string commandText, Func<DbDataReader, T> returnFunc)
    {
        DbConnection dbConnection = database.Database.GetDbConnection();

        await using DbCommand cmd = dbConnection.CreateCommand();
        cmd.CommandText = commandText;
        cmd.Transaction = GetDbTransaction(database.Database.CurrentTransaction);

        await using DbDataReader reader = await cmd.ExecuteReaderAsync();
        List<T> items = new();

        if (!reader.HasRows) return default;

        while (await reader.ReadAsync())
        {
            items.Add(returnFunc(reader));
        }

        return items;
    }

    private static DbTransaction GetDbTransaction(IDbContextTransaction dbContextTransaction)
    {
        if (dbContextTransaction is not IInfrastructure<DbTransaction> accessor)
        {
            throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
        }

        return accessor.GetInfrastructure();
    }
}