using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.PlayerData;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class FixBrokenVersusScores : IMigrationTask
{
    public string Name() => "Cleanup versus scores";

    async Task<bool> IMigrationTask.Run(Database database)
    {
        foreach (Score score in database.Scores)
        {
            if (!score.PlayerIdCollection.Contains(':')) continue;

            score.PlayerIdCollection = score.PlayerIdCollection.Replace(':', ',');
        }

        await database.SaveChangesAsync();
        return true;
    }
}