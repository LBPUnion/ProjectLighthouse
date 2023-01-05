using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class CleanupDuplicateScoresMigration : IMigrationTask
{
    public string Name() => "Cleanup duplicate scores";

    public async Task<bool> Run(Database database)
    {
        List<int> duplicateScoreIds = new();
        // The original score should have the lowest score id
        foreach (Score score in database.Scores.OrderBy(s => s.ScoreId)
                     .ToList()
                     .Where(score => !duplicateScoreIds.Contains(score.ScoreId)))
        {
            foreach (Score other in database.Scores.Where(s =>
                         s.Points == score.Points &&
                         s.Type == score.Type &&
                         s.SlotId == score.SlotId &&
                         s.ScoreId != score.ScoreId &&
                         s.ChildSlotId == score.ChildSlotId))
            {
                if (score.PlayerIds.Length != other.PlayerIds.Length || score.PlayerIds.Except(other.PlayerIds).Any())
                    continue;

                database.Scores.Remove(other);
                duplicateScoreIds.Add(other.ScoreId);
            }
        }

        await database.SaveChangesAsync();

        return false;
    }

}