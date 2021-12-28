#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Categories;
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

            List<Category> categories = new()
            {
                new TeamPicksCategory(),
                new NewestLevelsCategory(),
                new QueueCategory(user),
            };

            string categoriesSerialized = categories.Aggregate(string.Empty, (current, category) => current + category.Serialize(this.database));

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
                            "total", categories.Count
                        },
                    }
                )
            );
        }
    }
}