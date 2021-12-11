using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Maintenance.MaintenanceJobs
{
    public class FixAllBrokenPlayerRequirementsMaintenanceJob : IMaintenanceJob
    {
        private readonly Database database = new();

        public string Name() => "Fix All Broken Player Requirements";
        public string Description() => "Some LBP1 levels may report that they are designed for 0 players. This job will fix that.";
        public async Task Run()
        {
            int count = 0;
            await foreach (Slot slot in this.database.Slots)
                if (slot.MinimumPlayers == 0 || slot.MaximumPlayers == 0)
                {
                    slot.MinimumPlayers = 1;
                    slot.MaximumPlayers = 4;

                    Console.WriteLine($"Fixed slotId {slot.SlotId}");
                    count++;
                }

            await this.database.SaveChangesAsync();

            Console.WriteLine($"Fixed {count} broken player requirements.");
        }
    }
}