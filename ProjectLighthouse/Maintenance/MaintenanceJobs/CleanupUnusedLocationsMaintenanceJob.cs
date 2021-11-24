using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.Maintenance.MaintenanceJobs
{
    public class CleanupUnusedLocationsMaintenanceJob : IMaintenanceJob
    {
        private readonly Database database = new();
        public string Name() => "Cleanup Unused Locations";
        public string Description() => "Cleanup unused locations in the database.";

        public Task Run()
        {
            List<int> usedLocationIds = new();

            usedLocationIds.AddRange(this.database.Slots.Select(slot => slot.LocationId));
            usedLocationIds.AddRange(this.database.Users.Select(user => user.LocationId));

            this.database.RemoveRange(this.database.Locations.Where(l => !usedLocationIds.Contains(l.Id)));
            return Task.CompletedTask;
        }
    }
}