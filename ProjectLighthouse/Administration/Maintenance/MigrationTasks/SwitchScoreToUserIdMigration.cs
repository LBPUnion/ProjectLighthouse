using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LBPUnion.ProjectLighthouse.Administration.Maintenance.MigrationTasks;

public class SwitchScoreToUserIdMigration : MigrationTask
{
    #region DB entity replication stuff

    [Table("Scores")]
    private class OldScoreEntity
    {
        [Key]
        public int ScoreId { get; set; }

        public int SlotId { get; set; }

        [ForeignKey(nameof(SlotId))]
        public SlotEntity Slot { get; set; }

        public int ChildSlotId { get; set; }

        public int Type { get; set; }

        public string PlayerIdCollection { get; set; }

        [NotMapped]
        public string[] PlayerIds
        {
            get => this.PlayerIdCollection.Split(",");
            set => this.PlayerIdCollection = string.Join(',', value);
        }

        public int UserId { get; set; }

        public int Points { get; set; }

        public long Timestamp { get; set; }
    }

    private sealed class CustomDbContext : DbContext
    {
        public CustomDbContext(DbContextOptions<CustomDbContext> options) : base(options)
        { }

        public DbSet<OldScoreEntity> Scores { get; set; }
        public DbSet<SlotEntity> Slots { get; set; }
        public DbSet<UserEntity> Users { get; set; }
    }

    #endregion

    public override string Name() => "20230628023610_AddUserIdAndTimestampToScore";

    public override MigrationHook HookType() => MigrationHook.After;

    private List<SlotEntity> GetAllSlots(DatabaseContext database)
    {
        return null;
    }

    public override async Task<bool> Run(DatabaseContext db)
    {
        DbContextOptionsBuilder<CustomDbContext> builder = new();
        builder.UseMySql(ServerConfiguration.Instance.DbConnectionString,
            MySqlServerVersion.LatestSupportedServerVersion);

        int[] scoreTypes =
        {
            1, 2, 3, 4, 7,
        };
        CustomDbContext database = new(builder.Options);
        List<ScoreEntity> newScores = new();
        // Get all slots with at least 1 score
        foreach (SlotEntity slot in await database.Slots
                     .Where(s => database.Scores.Count(score => s.SlotId == score.SlotId) > 0)
                     .ToListAsync())
        {
            foreach (int type in scoreTypes)
            {
                newScores.AddRange(await FixScores(database, slot, type));
            }
        }

        return true;
    }

    private static Dictionary<string, int> CreateHighestScores(CustomDbContext database, List<OldScoreEntity> scores)
    {
        Dictionary<string, int> maxPointsByPlayer = new();
        foreach (OldScoreEntity score in scores)
        {
            string[] players = score.PlayerIds;
            foreach (string player in players)
            {
                if (!database.Users.Any(u => u.Username == player)) continue;

                _ = maxPointsByPlayer.TryGetValue(player, out int highestScore);
                highestScore = Math.Max(highestScore, score.Points);
                maxPointsByPlayer[player] = highestScore;
            }
        }

        return maxPointsByPlayer;
    }

    private static async Task<List<ScoreEntity>> FixScores(CustomDbContext database, SlotEntity slot, int scoreType)
    {
        //TODO create a map of all players with scores submitted, then find their highest score for this type and create a new score
        List<ScoreEntity> newScores = new();

        // Loop over all scores for this level grouped by ChildSlotId (to account for adventure levels)
        foreach (IGrouping<int, OldScoreEntity> group in database.Scores.Where(s => s.SlotId == slot.SlotId)
                     .Where(s => s.Type == scoreType)
                     .GroupBy(s => s.ChildSlotId))
        {
            Dictionary<string, int> highestScores = CreateHighestScores(database, group.ToList());
            foreach (KeyValuePair<string, int> kvp in highestScores)
            {
                int userId = await database.Users.Where(u => u.Username == kvp.Key).Select(u => u.UserId).FirstAsync();
                ScoreEntity scoreEntity = new()
                {
                    UserId = userId,
                    SlotId = slot.SlotId,
                    ChildSlotId = group.Key,
                    Points = kvp.Value,
                    Timestamp = TimeHelper.TimestampMillis,
                    Type = scoreType,
                };
                newScores.Add(scoreEntity);
            }
        }

        return newScores;
    }

}