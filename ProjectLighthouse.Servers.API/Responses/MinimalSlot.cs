using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public struct MinimalSlot
{
    public int SlotId { get; set; }
    public string Name { get; set; }
    public string IconHash { get; set; }
    public bool TeamPick { get; set; }
    public bool IsAdventure { get; set; }
    public GameVersion GameVersion { get; set; }
    #if DEBUG
    public long FirstUploaded { get; set; }
    #endif

    public static MinimalSlot FromSlot(Slot slot)
        => new()
        {
            SlotId = slot.SlotId,
            Name = slot.Name,
            IconHash = slot.IconHash,
            TeamPick = slot.TeamPick,
            IsAdventure = slot.IsAdventurePlanet,
            GameVersion = slot.GameVersion,
            #if DEBUG
            FirstUploaded = slot.FirstUploaded,
            #endif
        };
}