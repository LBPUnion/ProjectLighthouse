#nullable enable
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class CleanupSanitizedStrings : MigrationTask
{
    public override string Name() => "Cleanup Sanitized strings";

    public override async Task<bool> Run(DatabaseContext database)
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

        foreach (object obj in objsToBeSanitized)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType != typeof(string)) continue;

                string? before = (string?)property.GetValue(obj);

                if (before == null) continue;

                string after = HttpUtility.HtmlDecode(before);
                if (before != after)
                {
                    property.SetValue(obj, after);
                }
            }
        }

        await database.SaveChangesAsync();
        return true;
    }
}