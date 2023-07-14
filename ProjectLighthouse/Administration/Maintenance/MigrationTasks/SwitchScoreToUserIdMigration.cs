using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class SwitchScoreToUserIdMigration : MigrationTask
{
    #region DB entity replication stuff

    private class MigrationScore
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

    private class MigrationSlot
    {
        public int SlotId { get; set; }
    }

    private class MigrationUser
    {
        public int UserId { get; set; }

        public string Username { get; set; }
    }

    #endregion

    public override string Name() => "20230706020914_DropPlayerIdCollectionAndAddUserForeignKey";

    public override MigrationHook HookType() => MigrationHook.Before;

    private static async Task<List<MigrationScore>> GetAllScores(DbContext database)
    {
        return await MigrationHelper.GetAllObjects(database,
            "select * from Scores",
            reader => new MigrationScore
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

    private static async Task<List<MigrationSlot>> GetAllSlots(DbContext database)
    {
        return await MigrationHelper.GetAllObjects(database,
            "select SlotId from Slots",
            reader => new MigrationSlot
            {
                SlotId = reader.GetInt32("SlotId"),
            });
    }

    /// <summary>
    /// This function deletes all existing scores and inserts the new generated scores
    /// <para>All scores must be deleted because MySQL doesn't allow you to change primary keys</para>
    /// </summary>
    private static async Task ApplyFixedScores(DatabaseContext database, IReadOnlyList<ScoreEntity> newScores)
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

        StringBuilder insertionScript = new();
        // This is significantly faster than using standard EntityFramework Add and Save albeit a little wacky
        foreach (ScoreEntity score in newScores)
        {
            insertionScript.AppendLine($"""
                    insert into Scores (ScoreId, SlotId, Type, Points, ChildSlotId, Timestamp, UserId) 
                    values('{score.ScoreId}', '{score.SlotId}', '{score.Type}', '', '{score.Points}', '{score.ChildSlotId}', '{score.Timestamp}', '{score.UserId}');
                    """);
        }

        await database.Database.ExecuteSqlRawAsync(insertionScript.ToString());
    }

    public override async Task<bool> Run(DatabaseContext database)
    {
        int[] scoreTypes =
        {
            1, 2, 3, 4, 7,
        };
        ConcurrentBag<ScoreEntity> newScores = new();
        // Get all slots with at least 1 score
        List<MigrationSlot> slots = await GetAllSlots(database);
        List<MigrationScore> scores = await GetAllScores(database);

        // Don't run migration if there are no scores
        if (scores == null || scores.Count == 0) return true;

        List<MigrationUser> users = await GetAllUsers(database);

        ConcurrentQueue<(MigrationSlot slot, int type)> collection = new();
        foreach (MigrationSlot slot in slots.Where(s => scores.Any(score => s.SlotId == score.SlotId)))
        {
            foreach (int type in scoreTypes)
            {
                collection.Enqueue((slot, type));
            }
        }

        ConcurrentBag<Task> taskList = new();
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            Task task = Task.Run(() =>
            {
                while (collection.TryDequeue(out (MigrationSlot slot, int type) item))
                {
                    List<ScoreEntity> fixedScores = FixScores(users,
                            item.slot,
                            scores.Where(s => s.SlotId == item.slot.SlotId).Where(s => s.Type == item.type).ToList(),
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
    private static Dictionary<string, int> CreateHighestScores(List<MigrationScore> scores, IReadOnlyCollection<MigrationUser> userCache)
    {
        Dictionary<string, int> maxPointsByPlayer = new(StringComparer.InvariantCultureIgnoreCase);
        foreach (MigrationScore score in scores)
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
    private static IEnumerable<ScoreEntity> FixScores(IReadOnlyCollection<MigrationUser> userCache, MigrationSlot slot, IEnumerable<MigrationScore> scores, int scoreType)
    {
        return (
            from slotGroup in scores.GroupBy(s => s.ChildSlotId)
            let highestScores = CreateHighestScores(slotGroup.ToList(), userCache)
            from kvp in highestScores
            let userId = userCache.Where(u => u.Username == kvp.Key).Select(u => u.UserId).First()
            select new ScoreEntity
            {
                UserId = userId,
                SlotId = slot.SlotId,
                ChildSlotId = slotGroup.Key,
                Points = kvp.Value,
                Timestamp = TimeHelper.TimestampMillis,
                Type = scoreType,
            }).ToList();
    }
}