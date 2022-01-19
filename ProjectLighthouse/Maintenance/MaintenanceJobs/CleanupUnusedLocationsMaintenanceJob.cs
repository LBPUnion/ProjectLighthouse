using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Profiles;

namespace LBPUnion.ProjectLighthouse.Maintenance.MaintenanceJobs;

public class CleanupUnusedLocationsMaintenanceJob : IMaintenanceJob
{
    private readonly Database database = new();
    public string Name() => "Cleanup Unused Locations";
    public string Description() => "Cleanup unused locations in the database.";

    public async Task Run()
    {
        List<int> usedLocationIds = new();

        usedLocationIds.AddRange(this.database.Slots.Select(slot => slot.LocationId));
        usedLocationIds.AddRange(this.database.Users.Select(user => user.LocationId));

        IQueryable<Location> locationsToRemove = this.database.Locations.Where(l => !usedLocationIds.Contains(l.Id));

        foreach (Location location in locationsToRemove)
        {
            Console.WriteLine("Removing location " + location.Id);
            this.database.Locations.Remove(location);
        }

        await this.database.SaveChangesAsync();
    }
}