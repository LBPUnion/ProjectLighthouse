using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using LBPUnion.ProjectLighthouse.Types.Resources;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class CleanupSlotVersionMismatchMigration : IMigrationTask
{
    public string Name() => "Cleanup slot versions";

    async Task<bool> IMigrationTask.Run(DatabaseContext database)
    {
        foreach (SlotEntity slot in database.Slots)
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