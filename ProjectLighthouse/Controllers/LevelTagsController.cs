using System;
using Microsoft.AspNetCore.Mvc;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/tags")]
    [Produces("text/plain")]
    public class LevelTagsController : ControllerBase {
        [HttpGet]
        public IActionResult Get() {
            string[] tags = Enum.GetNames(typeof(LevelTags));

            int i = 0;
            foreach(string tag in tags) {
                tags[i] = $"TAG_{tag.Replace("_", "-")}";
                i++;
            }

            return this.Ok(string.Join(",", tags));
        }
    }
}