using System.Collections.Generic;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class CleanupXmlInjectionMigration : IMigrationTask
{
    public string Name() => "Cleanup XML injections";

    // Weird, but required. Thanks, hejlsberg.
    async Task<bool> IMigrationTask.Run(Database database)
    {
        List<object> objsToBeSanitized = new();
        
        // Store all the objects we need to sanitize in a list.
        // The alternative here is to loop through every table, but thats a ton of code...
        objsToBeSanitized.AddRange(database.Slots);
        objsToBeSanitized.AddRange(database.Reviews);
        objsToBeSanitized.AddRange(database.Comments);
        objsToBeSanitized.AddRange(database.Scores);
        objsToBeSanitized.AddRange(database.Users);
        objsToBeSanitized.AddRange(database.Photos);
        objsToBeSanitized.AddRange(database.Reports);
        
        foreach (object obj in objsToBeSanitized) SanitizationHelper.SanitizeStringsInClass(obj);

        await database.SaveChangesAsync();
        return true;
    }
}