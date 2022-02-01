#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Api;

public class GetUserEndpoint : ApiEndpoint
{
    private readonly Database database;

    public GetUserEndpoint(Database database)
    {
        this.database = database;
    }

    [HttpGet("user/{id:int}")]
    public async Task<IActionResult> OnGet(int id)
    {
        User? user = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return this.NotFound();

        return this.Ok(user);
    }
}