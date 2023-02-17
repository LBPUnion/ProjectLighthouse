#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Admin;

[ApiController]
[Route("/admin")]
public class AdminPanelController : ControllerBase
{
    private readonly DatabaseContext database;

    public AdminPanelController(DatabaseContext database)
    {
        this.database = database;
    }
}