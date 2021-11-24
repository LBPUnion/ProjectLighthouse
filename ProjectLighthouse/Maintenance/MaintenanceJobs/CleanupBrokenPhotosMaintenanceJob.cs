using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;

namespace LBPUnion.ProjectLighthouse.Maintenance.MaintenanceJobs
{
    public class CleanupBrokenPhotosMaintenanceJob : IMaintenanceJob
    {
        private readonly Database database = new();
        public string Name() => "Cleanup Broken Photos";
        public string Description() => "Deletes all photos that have missing assets.";

        [SuppressMessage("ReSharper", "LoopCanBePartlyConvertedToQuery")]
        public async Task Run()
        {
            foreach (Photo photo in this.database.Photos)
            {
                bool hashNullOrEmpty = string.IsNullOrEmpty
                                           (photo.LargeHash) ||
                                       string.IsNullOrEmpty(photo.MediumHash) ||
                                       string.IsNullOrEmpty(photo.SmallHash) ||
                                       string.IsNullOrEmpty(photo.PlanHash);

                bool allHashesDontExist = FileHelper.ResourcesNotUploaded(photo.LargeHash, photo.MediumHash, photo.SmallHash, photo.PlanHash).Length != 0;

                if (hashNullOrEmpty || allHashesDontExist)
                {
                    Console.WriteLine
                    (
                        $"Removing photo (id: {photo.PhotoId}): {nameof(hashNullOrEmpty)}: {hashNullOrEmpty}, {nameof(allHashesDontExist)}: {allHashesDontExist}"
                    );
                    this.database.Photos.Remove(photo);
                }
            }

            await this.database.SaveChangesAsync();
        }
    }
}