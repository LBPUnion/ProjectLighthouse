using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class CleanupDuplicateScoresMigration : IMigrationTask
{
    public string Name() => "Cleanup duplicate scores";

    public async Task<bool> Run(DatabaseContext database)
    {
        List<int> duplicateScoreIds = new();
        // The original score should have the lowest score id
        foreach (ScoreEntity score in database.Scores.OrderBy(s => s.ScoreId)
                     .ToList()
                     .Where(score => !duplicateScoreIds.Contains(score.ScoreId)))
        {
            foreach (ScoreEntity other in database.Scores.Where(s =>
                         s.Points == score.Points &&
                         s.Type == score.Type &&
                         s.SlotId == score.SlotId &&
                         s.ScoreId != score.ScoreId &&
                         s.ChildSlotId == score.ChildSlotId &&
                         s.ScoreId > score.ScoreId))
            {
                if (score.PlayerIds.Length != other.PlayerIds.Length)
                    continue;

                HashSet<string> hashSet = new(score.PlayerIds);

                if (!other.PlayerIds.All(hashSet.Contains)) continue;

                Logger.Info($"Removing score with id {other.ScoreId}, slotId={other.SlotId} main='{score.PlayerIdCollection}', duplicate={other.PlayerIdCollection}", LogArea.Score);
                database.Scores.Remove(other);
                duplicateScoreIds.Add(other.ScoreId);
            }
        }

        Logger.Info($"Removed a total of {duplicateScoreIds.Count} duplicate scores", LogArea.Score);
        await database.SaveChangesAsync();

        return true;
    }

}