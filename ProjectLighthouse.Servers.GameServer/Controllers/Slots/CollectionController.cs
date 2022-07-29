#nullable enable
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Levels.Categories;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class CollectionController : ControllerBase
{
    private readonly Database database;

    public CollectionController(Database database)
    {
        this.database = database;
    }

    [HttpGet("user/{username}/playlists")]
    public IActionResult GetUserPlaylists(string username) => this.Ok();

    [HttpGet("searches")]
    [HttpGet("genres")]
    public async Task<IActionResult> GenresAndSearches()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        string categoriesSerialized = CategoryHelper.Categories.Aggregate
        (
            string.Empty,
            (current, category) =>
            {
                string serialized;

                if (category is CategoryWithUser categoryWithUser) serialized = categoryWithUser.Serialize(this.database, user);
                else serialized = category.Serialize(this.database);

                return current + serialized;
            }
        );

        categoriesSerialized += LbpSerializer.StringElement("text_search", LbpSerializer.StringElement("url", "/slots/searchLBP3"));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement
            (
                "categories",
                categoriesSerialized,
                new Dictionary<string, object>
                {
                    {
                        "hint", ""
                    },
                    {
                        "hint_start", 1
                    },
                    {
                        "total", CategoryHelper.Categories.Count
                    },
                }
            )
        );
    }

    [HttpGet("searches/{endpointName}")]
    public async Task<IActionResult> GetCategorySlots(string endpointName, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        Category? category = CategoryHelper.Categories.FirstOrDefault(c => c.Endpoint == endpointName);
        if (category == null) return this.NotFound();

        Logger.Debug("Found category " + category, LogArea.Category);

        List<Slot> slots;
        int totalSlots;

        if (category is CategoryWithUser categoryWithUser)
        {
            slots = categoryWithUser.GetSlots(this.database, user, pageStart, pageSize).ToList();
            totalSlots = categoryWithUser.GetTotalSlots(this.database, user);
        }
        else
        {
            slots = category.GetSlots(this.database, pageStart, pageSize).ToList();
            totalSlots = category.GetTotalSlots(this.database);
        }

        string slotsSerialized = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize(gameToken.GameVersion));

        return this.Ok
        (
            LbpSerializer.TaggedStringElement
            (
                "results",
                slotsSerialized,
                new Dictionary<string, object>
                {
                    {
                        "total", totalSlots
                    },
                    {
                        "hint_start", pageStart + pageSize
                    },
                }
            )
        );
    }
}