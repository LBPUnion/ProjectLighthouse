using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class SwitchScoreToUserIdMigration : MigrationTask
{
    #region DB entity replication stuff

    private class PostMigrationScore
    {
        public int ScoreId { get; set; }

        public int SlotId { get; set; }

        public int ChildSlotId { get; set; }

        public int Type { get; set; }

        public int UserId { get; set; }

        public int Points { get; set; }

        public long Timestamp { get; set; }
    }

    private class PreMigrationScore
    {
        public int ScoreId { get; set; }

        public int SlotId { get; set; }

        public int ChildSlotId { get; set; }

        public int Type { get; set; }

        public string PlayerIdCollection { get; set; }

        [NotMapped]
        public IEnumerable<string> PlayerIds => this.PlayerIdCollection.Split(",");

        public int Points { get; set; }
    }

    private class MigrationUser
    {
        public int UserId { get; set; }

        public string Username { get; set; }
    }

    #endregion

    public override string Name() => "20230706020914_DropPlayerIdCollectionAndAddUserForeignKey";

    public override MigrationHook HookType() => MigrationHook.Before;

    private static async Task<List<PreMigrationScore>> GetAllScores(DbContext database)
    {
        return await MigrationHelper.GetAllObjects(database,
            "select * from Scores",
            reader => new PreMigrationScore
            {
                ScoreId = reader.GetInt32("ScoreId"),
                SlotId = reader.GetInt32("SlotId"),
                ChildSlotId = reader.GetInt32("ChildSlotId"),
                Type = reader.GetInt32("Type"),
                PlayerIdCollection = reader.GetString("PlayerIdCollection"),
                Points = reader.GetInt32("Points"),
            });
    }

    private static async Task<List<MigrationUser>> GetAllUsers(DbContext database)
    {
        return await MigrationHelper.GetAllObjects(database,
            "select UserId, Username from Users",
            reader => new MigrationUser
            {
                UserId = reader.GetInt32("UserId"),
                Username = reader.GetString("Username"),
            });
    }

    private static async Task<List<int>> GetAllSlots(DbContext database)
    {
        return await MigrationHelper.GetAllObjects(database,
            "select SlotId from Slots",
            reader => reader.GetInt32("SlotId"));
    }

    /// <summary>
    /// This function deletes all existing scores and inserts the new generated scores
    /// <para>All scores must be deleted because MySQL doesn't allow you to change primary keys</para>
    /// </summary>
    private static async Task ApplyFixedScores(DatabaseContext database, IReadOnlyList<PostMigrationScore> newScores)
    {
        // Re-order scores (The order doesn't make any difference but since we're already deleting everything we may as well) 
        newScores = newScores.OrderByDescending(s => s.SlotId)
            .ThenByDescending(s => s.ChildSlotId)
            .ThenByDescending(s => s.Type)
            .ToList();

        // Set IDs for new scores
        for (int i = 1; i < newScores.Count; i++)
        {
            newScores[i].ScoreId = i;
        }
        // Delete all existing scores
        await database.Scores.ExecuteDeleteAsync();

        long timestamp = TimeHelper.TimestampMillis;

        // This is significantly faster than using standard EntityFramework Add and Save albeit a little wacky
        foreach (PostMigrationScore[] scoreChunk in newScores.Chunk(50_000))
        {
            StringBuilder insertionScript = new();
            foreach (PostMigrationScore score in scoreChunk)
            {
                insertionScript.AppendLine($"""
                    insert into Scores (ScoreId, SlotId, Type, Points, ChildSlotId, Timestamp, UserId) 
                    values('{score.ScoreId}', '{score.SlotId}', '{score.Type}', '{score.Points}', '{score.ChildSlotId}', '{timestamp}', '{score.UserId}');
                    """);
            }

            await database.Database.ExecuteSqlRawAsync(insertionScript.ToString());
        }
    }

    public override async Task<bool> Run(DatabaseContext database)
    {
        int[] scoreTypes =
        {
            1, 2, 3, 4, 7,
        };
        ConcurrentBag<PostMigrationScore> newScores = new();

        List<int> slotIds = await GetAllSlots(database);
        List<PreMigrationScore> scores = await GetAllScores(database);

        // Don't run migration if there are no scores
        if (scores == null || scores.Count == 0) return true;

        List<MigrationUser> users = await GetAllUsers(database);

        ConcurrentQueue<(int slotId, int type)> collection = new();
        foreach (int slotId in slotIds.Where(id => scores.Any(score => id == score.SlotId)))
        {
            foreach (int type in scoreTypes)
            {
                collection.Enqueue((slotId, type));
            }
        }

        ConcurrentBag<Task> taskList = new();
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            Task task = Task.Run(() =>
            {
                while (collection.TryDequeue(out (int slotId, int type) item))
                {
                    List<PostMigrationScore> fixedScores = FixScores(users,
                            item.slotId,
                            scores.Where(s => s.SlotId == item.slotId).Where(s => s.Type == item.type).ToList(),
                            item.type)
                        .ToList();
                    fixedScores.AsParallel().ForAll(score => newScores.Add(score));
                }
            });
            taskList.Add(task);
        }

        await Task.WhenAll(taskList);

        await ApplyFixedScores(database, newScores.ToList());

        return true;
    }

    /// <summary>
    /// This function takes in a list of scores and creates a map of players and their highest score 
    /// </summary>
    private static Dictionary<string, int> CreateHighestScores(List<PreMigrationScore> scores, IReadOnlyCollection<MigrationUser> userCache)
    {
        Dictionary<string, int> maxPointsByPlayer = new(StringComparer.InvariantCultureIgnoreCase);
        foreach (PreMigrationScore score in scores)
        {
            IEnumerable<string> players = score.PlayerIds;
            foreach (string player in players)
            {
                // Remove non existent users to ensure foreign key constraint
                if (userCache.All(u => u.Username != player)) continue;

                _ = maxPointsByPlayer.TryGetValue(player, out int highestScore);
                highestScore = Math.Max(highestScore, score.Points);
                maxPointsByPlayer[player] = highestScore;
            }
        }

        return maxPointsByPlayer;
    }

    /// <summary>
    /// This function groups slots by ChildSlotId to account for adventure scores and then for each user
    /// finds their highest score on that level and adds a new Score 
    /// </summary>
    private static IEnumerable<PostMigrationScore> FixScores(IReadOnlyCollection<MigrationUser> userCache, int slotId, IEnumerable<PreMigrationScore> scores, int scoreType)
    {
        return (
            from slotGroup in scores.GroupBy(s => s.ChildSlotId)
            let highestScores = CreateHighestScores(slotGroup.ToList(), userCache)
            from kvp in highestScores
            let userId = userCache.Where(u => u.Username == kvp.Key).Select(u => u.UserId).First()
            select new PostMigrationScore
            {
                UserId = userId,
                SlotId = slotId,
                ChildSlotId = slotGroup.Key,
                Points = kvp.Value,
                // This gets set before insertion
                Timestamp = 0L,
                Type = scoreType,
            }).ToList();
    }
}