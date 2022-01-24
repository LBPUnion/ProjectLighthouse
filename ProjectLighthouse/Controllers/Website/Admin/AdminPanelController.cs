#nullable enable
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
}