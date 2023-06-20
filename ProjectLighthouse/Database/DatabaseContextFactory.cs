using Microsoft.EntityFrameworkCore.Design;

namespace LBPUnion.ProjectLighthouse.Database;

/// <summary>
/// This class is only used for the code-first migration builder to detect the DbContext
/// </summary>
// ReSharper disable once UnusedType.Global
public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args) => DatabaseContext.CreateNewInstance();
}