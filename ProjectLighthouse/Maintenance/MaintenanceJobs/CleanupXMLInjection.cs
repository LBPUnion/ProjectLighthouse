#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using LBPUnion.ProjectLighthouse.Types.Reports;
using LBPUnion.ProjectLighthouse.Types.Reviews;

namespace LBPUnion.ProjectLighthouse.Maintenance.MaintenanceJobs;

public class CleanupXmlInjection : IMaintenanceJob
{
    private readonly Database database = new();
    public string Name() => "Cleanup unsanitized XML";
    public string Description() => "Sanitizes all strings in levels, reviews, comments, users, and scores";

    public async Task Run()
    {
        foreach (Slot slot in this.database.Slots)
        {
            ReflectionHelper.sanitizeStringsInClass(slot);
        }
        
        foreach (Review review in this.database.Reviews)
        {
            ReflectionHelper.sanitizeStringsInClass(review);
        }

        foreach (Comment comment in this.database.Comments)
        {
            ReflectionHelper.sanitizeStringsInClass(comment);
        }
        
        foreach (Score score in this.database.Scores)
        {
            ReflectionHelper.sanitizeStringsInClass(score);
        }
        
        foreach (User user in this.database.Users)
        {
            ReflectionHelper.sanitizeStringsInClass(user);
        }

        foreach (Photo photo in this.database.Photos)
        {
            ReflectionHelper.sanitizeStringsInClass(photo);
        }

        foreach (GriefReport report in this.database.Reports)
        {
            ReflectionHelper.sanitizeStringsInClass(report);
        }

        await this.database.SaveChangesAsync();
    }
}