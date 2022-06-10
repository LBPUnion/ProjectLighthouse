#nullable enable
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Admin;

[ApiController]
[Route("/admin")]
public class AdminPanelController : ControllerBase
{
    private readonly Database database;

    public AdminPanelController(Database database)
    {
        this.database = database;
    }
}