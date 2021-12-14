#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Files;

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
                bool hashNullOrEmpty = false;
                bool noHashesExist = false;
                bool largeHashIsInvalidFile = false;

                hashNullOrEmpty = string.IsNullOrEmpty
                                      (photo.LargeHash) ||
                                  string.IsNullOrEmpty(photo.MediumHash) ||
                                  string.IsNullOrEmpty(photo.SmallHash) ||
                                  string.IsNullOrEmpty(photo.PlanHash);
                if (hashNullOrEmpty) goto removePhoto;

                List<string> hashes = new()
                {
                    photo.LargeHash,
                    photo.MediumHash,
                    photo.SmallHash,
                    photo.PlanHash,
                };

                noHashesExist = FileHelper.ResourcesNotUploaded(hashes.ToArray()).Length != 0;
                if (noHashesExist) goto removePhoto;

                LbpFile? file = LbpFile.FromHash(photo.LargeHash);
//                Console.WriteLine(file.FileType, );
                if (file == null || file.FileType != LbpFileType.Jpeg && file.FileType != LbpFileType.Png)
                {
                    largeHashIsInvalidFile = true;
                    goto removePhoto;
                }

                continue;

                removePhoto:

                Console.WriteLine
                (
                    $"Removing photo (id: {photo.PhotoId}): " +
                    $"{nameof(hashNullOrEmpty)}: {hashNullOrEmpty}, " +
                    $"{nameof(noHashesExist)}: {noHashesExist}, " +
                    $"{nameof(largeHashIsInvalidFile)}: {largeHashIsInvalidFile}"
                );

                this.database.Photos.Remove(photo);
            }

            await this.database.SaveChangesAsync();
        }
    }
}