#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers.Website.Admin;

[ApiController]
[Route("/admin")]
public class AdminPanelController : ControllerBase
{
    private readonly Database database;

    public AdminPanelController(Database database)
    {
        this.database = database;
    }

    [HttpGet("testWebhook")]
    public async Task<IActionResult> TestWebhook()
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        await WebhookHelper.SendWebhook("Testing 123", "Someone is testing the Discord webhook from the admin panel.");
        return this.Redirect("/admin");
    }
}