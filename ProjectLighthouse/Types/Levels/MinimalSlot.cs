namespace LBPUnion.ProjectLighthouse.Types.Levels;

public struct MinimalSlot
{
    public int SlotId { get; set; }
    public string Name { get; set; }
    public string IconHash { get; set; }
    public bool TeamPick { get; set; }
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
            GameVersion = slot.GameVersion,
            #if DEBUG
            FirstUploaded = slot.FirstUploaded,
            #endif
        };
}