using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MaintenanceJobs;

public class DeleteAllTokensMaintenanceJob : IMaintenanceJob
{
    private readonly DatabaseContext database = DatabaseContext.CreateNewInstance();

    public string Name() => "Delete ALL Tokens";
    public string Description() => "Deletes ALL game tokens and web tokens.";
    public async Task Run()
    {
        this.database.GameTokens.RemoveRange(this.database.GameTokens);
        this.database.WebTokens.RemoveRange(this.database.WebTokens);

        await this.database.SaveChangesAsync();

        Console.WriteLine("Deleted ALL tokens.");
    }
}