using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Servers.Website.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[ApiController]
public class AutoDiscoverController: ControllerBase
{
    [ResponseCache(Duration = 86400)]
    [HttpGet("autodiscover")]
    [Produces("application/json")]
    public IActionResult AutoDiscover()
    {
        AutoDiscoverResponse resp = new()
        {
            Version = 3,
            Url = ServerConfiguration.Instance.GameApiExternalUrl,
            ServerBrand = ServerConfiguration.Instance.Customization.ServerName,
            UsesCustomDigestKey = false,
            BannerImageUrl = null,
            ServerDescription = ServerConfiguration.Instance.Customization.ServerDescription,
        };
        return this.Ok(resp);
    }
}