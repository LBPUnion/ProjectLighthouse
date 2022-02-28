using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Helpers;

public static class SlotTypeHelper
{

    public static SlotType ParseSlotType(string str)
    {
        return str switch
        {
            "developer" => SlotType.Developer,
            "user" => SlotType.User,
            _ => SlotType.Unknown,
        };
    }

    public static bool IsValidStoryLevel(int id) => storyModeIds.Contains(id);

    public static string SlotTypeToString(SlotType type)
    {
        return type switch
        {
            SlotType.Developer => "developer",
            SlotType.User => "user",
            _ => "unknown",
        };
    }

    public static async Task<string> SerializeDeveloperSlot(Database db, int id)
    {
        int comments = await db.Comments.CountAsync(c => c.Type == CommentType.Level && c.TargetId == id && c.SlotType == SlotType.Developer);

        int photos = await db.Photos.CountAsync(p => p.SlotId == id && p.SlotType == SlotType.Developer);

        string slotData = LbpSerializer.StringElement("id", id) +
                          LbpSerializer.StringElement("playerCount", 0) +
                          LbpSerializer.StringElement("commentCount", comments) +
                          LbpSerializer.StringElement("photoCount", photos);

        return LbpSerializer.TaggedStringElement("slot", slotData, "type", "developer");
    }

    // this may not actually be feasible
    private static int[] storyModeIds =
    {
        -1,
    };

}