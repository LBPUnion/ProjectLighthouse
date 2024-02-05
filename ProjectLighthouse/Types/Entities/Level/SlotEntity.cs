#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Level;

/// <summary>
///     A LittleBigPlanet level.
/// </summary>
public class SlotEntity
{
    [Key]
    public int SlotId { get; set; }

    public int InternalSlotId { get; set; }

    public SlotType Type { get; set; } = SlotType.User;

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public string IconHash { get; set; } = "";

    public bool IsAdventurePlanet { get; set; }

    public string RootLevel { get; set; } = "";

    public string ResourceCollection { get; set; } = "";

    [NotMapped]
    public string[]? Resources {
        get => this.ResourceCollection.Split(",", StringSplitOptions.RemoveEmptyEntries);
        set => this.ResourceCollection = string.Join(',', value ?? Array.Empty<string>());
    }

    /// <summary>
    ///     The location of the level on the user's earth
    ///     Stored as a single 64 bit unsigned integer but split into
    ///     2 unsigned 32 bit integers
    /// </summary>
    public ulong LocationPacked { get; set; }

    [NotMapped]
    public Location Location
    {
        get =>
            new()
            {
                X = (int)(this.LocationPacked >> 32),
                Y = (int)this.LocationPacked,
            };
        set => this.LocationPacked = (ulong)value.X << 32 | (uint)value.Y;
    }

    public int CreatorId { get; set; }

    [ForeignKey(nameof(CreatorId))]
    public UserEntity? Creator { get; set; }

    public bool InitiallyLocked { get; set; }
    
    public bool LockedByModerator { get; set; }

    public string LockedReason { get; set; } = "";

    public bool SubLevel { get; set; }

    public bool Lbp1Only { get; set; }

    public int Shareable { get; set; }

    public string AuthorLabels { get; set; } = "";

    public string[] LevelTags(DatabaseContext database)
    {
        if (this.GameVersion != GameVersion.LittleBigPlanet1) return Array.Empty<string>();

        return database.RatedLevels.Where(r => r.SlotId == this.SlotId && r.TagLBP1.Length > 0)
            .GroupBy(r => r.TagLBP1)
            .OrderByDescending(kvp => kvp.Count())
            .Select(kvp => kvp.Key)
            .ToArray();
    }

    public string BackgroundHash { get; set; } = "";

    public int MinimumPlayers { get; set; }

    public int MaximumPlayers { get; set; }

    public bool MoveRequired { get; set; }

    public long FirstUploaded { get; set; }

    public long LastUpdated { get; set; }

    public bool TeamPick { get; set; }

    public GameVersion GameVersion { get; set; }

    public string LevelType { get; set; } = "";

    public bool CrossControllerRequired { get; set; }

    public bool Hidden { get; set; }

    public string HiddenReason { get; set; } = "";

    public int PlaysLBP1 { get; set; }

    public int PlaysLBP1Complete { get; set; }

    public int PlaysLBP1Unique { get; set; }

    public int PlaysLBP2 { get; set; }

    public int PlaysLBP2Complete { get; set; }

    public int PlaysLBP2Unique { get; set; }

    public int PlaysLBP3 { get; set; }

    public int PlaysLBP3Complete { get; set; }

    public int PlaysLBP3Unique { get; set; }

    public bool CommentsEnabled { get; set; } = true;

    [NotMapped]
    public int Plays => this.PlaysLBP1 + this.PlaysLBP2 + this.PlaysLBP3;

    [NotMapped]
    public int PlaysUnique => this.PlaysLBP1Unique + this.PlaysLBP2Unique + this.PlaysLBP3Unique;

    [NotMapped]
    public int PlaysComplete => this.PlaysLBP1Complete + this.PlaysLBP2Complete + this.PlaysLBP3Complete;
}