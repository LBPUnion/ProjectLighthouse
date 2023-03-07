using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Maintenance;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class FixBrokenVersusScores : IMigrationTask
{
    public string Name() => "Cleanup versus scores";

    async Task<bool> IMigrationTask.Run(DatabaseContext database)
    {
        foreach (ScoreEntity score in database.Scores)
        {
            if (!score.PlayerIdCollection.Contains(':')) continue;

            score.PlayerIdCollection = score.PlayerIdCollection.Replace(':', ',');
        }

        await database.SaveChangesAsync();
        return true;
    }
}