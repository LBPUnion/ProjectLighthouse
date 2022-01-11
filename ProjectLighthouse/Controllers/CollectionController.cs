#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Categories;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers
{
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

            string categoriesSerialized = CollectionHelper.Categories.Aggregate
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
                            "total", CollectionHelper.Categories.Count
                        },
                    }
                )
            );
        }

        [HttpGet("searches/{endpointName}")]
        public async Task<IActionResult> GetCategorySlots(string endpointName, [FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            User? user = await this.database.UserFromGameRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Category? category = CollectionHelper.Categories.FirstOrDefault(c => c.Endpoint == endpointName);
            if (category == null) return this.NotFound();

            Logger.Log("Found category " + category, LoggerLevelCategory.Instance);

            List<Slot> slots = category.GetSlots(this.database, pageStart, pageSize).ToList();

            string slotsSerialized = slots.Aggregate(string.Empty, (current, slot) => current + slot.Serialize());

            return this.Ok
            (
                LbpSerializer.TaggedStringElement
                (
                    "results",
                    slotsSerialized,
                    new Dictionary<string, object>
                    {
                        {
                            "total", category.GetTotalSlots(this.database)
                        },
                        {
                            "hint_start", pageStart + pageSize
                        },
                    }
                )
            );
        }
    }
}