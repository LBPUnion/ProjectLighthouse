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

    public static SlotType getSlotTypeFromRequest(HttpRequest request) => getSlotTypeFromString(request.Path.ToString());

    public static SlotType getSlotTypeFromString(string str)
    {
        return str switch
        {
            _ when str.Contains("developer") => SlotType.Developer,
            _ when str.Contains("user") => SlotType.User,
            _ when str.Contains("pod") => SlotType.Pod,
            _ => SlotType.Unknown,
        };
    }

    public static bool isValidStoryLevel(int id) => StoryModeIds.Contains(id);

    public static string slotTypeToString(SlotType type)
    {
        return type switch
        {
            SlotType.Developer => "developer",
            SlotType.User => "user",
            SlotType.Pod => "pod",
            _ => "unknown",
        };
    }

    public static async Task<string> serializeDeveloperSlot(Database db, int id)
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
    private static int[] StoryModeIds =
    {
        -1,
    };

}