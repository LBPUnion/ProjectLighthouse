using System;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Maintenance.MaintenanceJobs;

public class DeleteAllTokensMaintenanceJob : IMaintenanceJob
{
    private readonly Database database = new();

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