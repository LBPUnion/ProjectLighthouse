using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Users;
using Newtonsoft.Json;

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

[JsonObject]
public struct ApiSlot
{
    public int SlotId { get; set; }
    public int InternalSlotId { get; set; }
    public SlotType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IconHash { get; set; }
    public bool IsAdventure { get; set; }
    public int CreatorId { get; set; }
    public bool InitiallyLocked { get; set; }
    public bool SubLevel { get; set; }
    public bool Lbp1Only { get; set; }
    public int Shareable { get; set; }
    public string AuthorLabels { get; set; }
    public string[] LevelTags { get; set; }
    public int MinimumPlayers { get; set; }
    public int MaximumPlayers { get; set; }
    public bool MoveRequired { get; set; }
    public long FirstUploaded { get; set; }
    public long LastUpdated { get; set; }
    public bool TeamPick { get; set; }
    public Location Location { get; set; }
    public GameVersion GameVersion { get; set; }
    public int Plays { get; set; }
    public int PlaysUnique { get; set; }
    public int PlaysComplete { get; set; }
    public bool CommentsEnabled { get; set; }
    public double AverageRating { get; set; }
    public string LevelType { get; set; }

    public static ApiSlot CreateFromEntity(SlotEntity slot, DatabaseContext context) =>
        new()
        {
            SlotId = slot.SlotId,
            InternalSlotId = slot.InternalSlotId,
            Type = slot.Type,
            Name = slot.Name,
            Description = slot.Description,
            IconHash = slot.IconHash,
            IsAdventure = slot.IsAdventurePlanet,
            CreatorId = slot.CreatorId,
            InitiallyLocked = slot.InitiallyLocked,
            SubLevel = slot.SubLevel,
            Lbp1Only = slot.Lbp1Only,
            Shareable = slot.Shareable,
            AuthorLabels = slot.AuthorLabels,
            LevelTags = slot.LevelTags(context),
            MinimumPlayers = slot.MinimumPlayers,
            MaximumPlayers = slot.MaximumPlayers,
            MoveRequired = slot.MoveRequired,
            FirstUploaded = slot.FirstUploaded,
            LastUpdated = slot.LastUpdated,
            TeamPick = slot.TeamPick,
            Location = slot.Location,
            GameVersion = slot.GameVersion,
            Plays = slot.Plays,
            PlaysUnique = slot.PlaysUnique,
            PlaysComplete = slot.PlaysComplete,
            CommentsEnabled = slot.CommentsEnabled,
            AverageRating = context.RatedLevels.Where(r => r.SlotId == slot.SlotId).Average(r => (double?)r.RatingLBP1) ?? 3.0,
            LevelType = slot.LevelType,
        };
}