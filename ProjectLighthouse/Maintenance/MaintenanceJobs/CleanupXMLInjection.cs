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
    public string Name() => "Sanitize user content";
    public string Description() => "Sanitizes all user-generated strings in levels, reviews, comments, users, and scores to prevent XML injection. Only needs to be run once.";

    public async Task Run()
    {
        foreach (Slot slot in this.database.Slots) SanitizationHelper.SanitizeStringsInClass(slot);
        
        foreach (Review review in this.database.Reviews) SanitizationHelper.SanitizeStringsInClass(review);

        foreach (Comment comment in this.database.Comments) SanitizationHelper.SanitizeStringsInClass(comment);

        foreach (Score score in this.database.Scores) SanitizationHelper.SanitizeStringsInClass(score);

        foreach (User user in this.database.Users) SanitizationHelper.SanitizeStringsInClass(user);

        foreach (Photo photo in this.database.Photos) SanitizationHelper.SanitizeStringsInClass(photo);

        foreach (GriefReport report in this.database.Reports) SanitizationHelper.SanitizeStringsInClass(report);

        await this.database.SaveChangesAsync();
    }
}