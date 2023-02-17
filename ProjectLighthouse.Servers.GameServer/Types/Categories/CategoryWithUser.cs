#nullable enable
using System.Diagnostics;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public abstract class CategoryWithUser : Category
{
    public abstract Slot? GetPreviewSlot(DatabaseContext database, User user);
    public override Slot? GetPreviewSlot(DatabaseContext database)
    {
        #if DEBUG
        Logger.Error("tried to get preview slot without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return null;
    }

    public abstract int GetTotalSlots(DatabaseContext database, User user);
    public override int GetTotalSlots(DatabaseContext database)
    {
        #if DEBUG
        Logger.Error("tried to get total slots without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return -1;
    }

    public abstract IEnumerable<Slot> GetSlots(DatabaseContext database, User user, int pageStart, int pageSize);
    public override IEnumerable<Slot> GetSlots(DatabaseContext database, int pageStart, int pageSize)
    {
        #if DEBUG
        Logger.Error("tried to get slots without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return new List<Slot>();
    }

    public new string Serialize(DatabaseContext database)
    {
        Logger.Error("tried to serialize without user on CategoryWithUser", LogArea.Category);
        return string.Empty;
    }

    public string Serialize(DatabaseContext database, User user)
    {
        Slot? previewSlot = this.GetPreviewSlot(database, user);

        string previewResults = "";
        if (previewSlot != null)
            previewResults = LbpSerializer.TaggedStringElement
            (
                "results",
                previewSlot.Serialize(),
                new Dictionary<string, object>
                {
                    {
                        "total", this.GetTotalSlots(database, user)
                    },
                    {
                        "hint_start", "2"
                    },
                }
            );

        return LbpSerializer.StringElement
        (
            "category",
            LbpSerializer.StringElement("name", this.Name) +
            LbpSerializer.StringElement("description", this.Description) +
            LbpSerializer.StringElement("url", this.IngameEndpoint) +
            (previewSlot == null ? "" : previewResults) +
            LbpSerializer.StringElement("icon", this.IconHash)
        );
    }
}