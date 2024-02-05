using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MaintenanceJobs;

public class DeleteAllTokensMaintenanceJob : IMaintenanceJob
{
    public string Name() => "Delete ALL Tokens";
    public string Description() => "Deletes ALL game tokens and web tokens.";

    public async Task Run()
    {
        await using DatabaseContext database = DatabaseContext.CreateNewInstance();
        await database.GameTokens.RemoveWhere(t => true);
        await database.WebTokens.RemoveWhere(t => true);
        Console.WriteLine(@"Deleted ALL tokens.");
    }
}