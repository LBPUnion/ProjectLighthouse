using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MaintenanceJobs;

public class CleanupUnusedPhotoSubjects : IMaintenanceJob
{
    private readonly Database database = new();
    public string Name() => "Cleanup Unused PhotoSubjects";
    public string Description() => "Cleanup unused photo subjects in the database.";

    public async Task Run()
    {
        List<string> subjectCollections = new();
        List<int> usedPhotoSubjectIds = new();

        subjectCollections.AddRange(this.database.Photos.Select(p => p.PhotoSubjectCollection));

        foreach (string idCollection in subjectCollections)
        {
            usedPhotoSubjectIds.AddRange(idCollection.Split(",").Where(x => int.TryParse(x, out _)).Select(int.Parse));
        }

        IQueryable<PhotoSubject> subjectsToRemove = this.database.PhotoSubjects.Where(p => !usedPhotoSubjectIds.Contains(p.PhotoSubjectId));

        foreach (PhotoSubject subject in subjectsToRemove)
        {
            Console.WriteLine(@"Removing subject " + subject.PhotoSubjectId);
            this.database.PhotoSubjects.Remove(subject);
        }

        await this.database.SaveChangesAsync();
    }
    
}