using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MaintenanceJobs;

public class CleanupSlotVersionMismatchMaintenanceJob : IMaintenanceJob
{
    private readonly Database _database = new();
    public string Name() => "Cleanup slot versions";
    public string Description() => "Cleans up any slots that may have been published under the wrong game version. Only needs to be run once.";

    public async Task Run()
    {
        foreach (Slot slot in this._database.Slots)
        {
            LbpFile rootLevel = LbpFile.FromHash(slot.RootLevel);
            if (rootLevel == null)
            {
                continue;
            }

            GameVersion slotVersion = FileHelper.ParseLevelVersion(rootLevel);
            if (slotVersion != GameVersion.Unknown)
            {
                slot.GameVersion = slotVersion;
            }
            
        }

        await this._database.SaveChangesAsync();
    }
}