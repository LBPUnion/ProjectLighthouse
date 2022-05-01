#nullable enable
using System.Collections.Generic;
using System.Diagnostics;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;

namespace LBPUnion.ProjectLighthouse.Types.Categories;

public abstract class CategoryWithUser : Category
{
    public abstract Slot? GetPreviewSlot(Database database, User user);
    public override Slot? GetPreviewSlot(Database database)
    {
        #if DEBUG
        Logger.LogError("tried to get preview slot without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return null;
    }

    public abstract int GetTotalSlots(Database database, User user);
    public override int GetTotalSlots(Database database)
    {
        #if DEBUG
        Logger.LogError("tried to get total slots without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return -1;
    }

    public abstract IEnumerable<Slot> GetSlots(Database database, User user, int pageStart, int pageSize);
    public override IEnumerable<Slot> GetSlots(Database database, int pageStart, int pageSize)
    {
        #if DEBUG
        Logger.LogError("tried to get slots without user on CategoryWithUser", LogArea.Category);
        if (Debugger.IsAttached) Debugger.Break();
        #endif
        return new List<Slot>();
    }

    public new string Serialize(Database database)
    {
        Logger.LogError("tried to serialize without user on CategoryWithUser", LogArea.Category);
        return string.Empty;
    }

    public string Serialize(Database database, User user)
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