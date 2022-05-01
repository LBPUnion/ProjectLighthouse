#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Files;

namespace LBPUnion.ProjectLighthouse.Maintenance.MaintenanceJobs;

public class CleanupBrokenPhotosMaintenanceJob : IMaintenanceJob
{
    private readonly Database database = new();
    public string Name() => "Cleanup Broken Photos";
    public string Description() => "Deletes all photos that have missing assets or invalid photo subjects.";

    [SuppressMessage("ReSharper", "LoopCanBePartlyConvertedToQuery")]
    public async Task Run()
    {
        foreach (Photo photo in this.database.Photos)
        {
            bool hashNullOrEmpty = false;
            bool noHashesExist = false;
            bool largeHashIsInvalidFile = false;
            bool tooManyPhotoSubjects = false;
            bool duplicatePhotoSubjects = false;
            bool takenInTheFuture = true;

            // Checks should generally be ordered in least computationally expensive to most.

            if (photo.Subjects.Count > 4)
            {
                tooManyPhotoSubjects = true;
                goto removePhoto;
            }

            if (photo.Timestamp > TimestampHelper.Timestamp)
            {
                takenInTheFuture = true;
                goto removePhoto;
            }

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

            List<int> subjectUserIds = new(4);
            foreach (PhotoSubject subject in photo.Subjects)
            {
                if (subjectUserIds.Contains(subject.UserId))
                {
                    duplicatePhotoSubjects = true;
                    goto removePhoto;
                }
                subjectUserIds.Add(subject.UserId);
            }

            LbpFile? file = LbpFile.FromHash(photo.LargeHash);
//                Console.WriteLine(file.FileType, );
            if (file == null || file.FileType != LbpFileType.Jpeg && file.FileType != LbpFileType.Png)
            {
                largeHashIsInvalidFile = true;
                goto removePhoto;
            }

            noHashesExist = FileHelper.ResourcesNotUploaded(hashes.ToArray()).Length != 0;
            if (noHashesExist) goto removePhoto;

            continue;

            removePhoto:

            Console.WriteLine
            (
                $"Removing photo (id: {photo.PhotoId}): " +
                $"{nameof(hashNullOrEmpty)}: {hashNullOrEmpty}, " +
                $"{nameof(noHashesExist)}: {noHashesExist}, " +
                $"{nameof(largeHashIsInvalidFile)}: {largeHashIsInvalidFile}, " +
                $"{nameof(tooManyPhotoSubjects)}: {tooManyPhotoSubjects}" +
                $"{nameof(duplicatePhotoSubjects)}: {duplicatePhotoSubjects}" +
                $"{nameof(takenInTheFuture)}: {takenInTheFuture}"
            );

            this.database.Photos.Remove(photo);
        }

        await this.database.SaveChangesAsync();
    }
}