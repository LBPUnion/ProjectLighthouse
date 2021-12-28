using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Serialization;
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
            List<Category> categories = new()
            {
                new TeamPicksCategory(),
                new TeamPicksCategory(),
                new NewestLevelsCategory(),
                new NewestLevelsCategory(),
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