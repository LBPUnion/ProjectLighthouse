using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Entities.Level;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Maintenance;
using LBPUnion.ProjectLighthouse.Resources;
using LBPUnion.ProjectLighthouse.Users;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class CleanupSlotVersionMismatchMigration : IMigrationTask
{
    public string Name() => "Cleanup slot versions";

    async Task<bool> IMigrationTask.Run(Database database)
    {
        foreach (Slot slot in database.Slots)
        {
            LbpFile rootLevel = LbpFile.FromHash(slot.RootLevel);
            if (rootLevel == null) continue;

            GameVersion slotVersion = FileHelper.ParseLevelVersion(rootLevel);

            if (slotVersion != GameVersion.Unknown) slot.GameVersion = slotVersion;
        }

        await database.SaveChangesAsync();
        return true;
    }
}