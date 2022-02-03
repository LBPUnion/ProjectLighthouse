#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Api;

/// <summary>
/// A collection of endpoints relating to users.
/// </summary>
public class UserEndpoints : ApiEndpointController
{
    private readonly Database database;

    public UserEndpoints(Database database)
    {
        this.database = database;
    }

    /// <summary>
    /// Gets a user and their information from the database.
    /// </summary>
    /// <param name="id">The ID of the user</param>
    /// <returns>The user</returns>
    /// <response code="200">The user, if successful.</response>
    /// <response code="404">The user could not be found.</response>
    [HttpGet("user/{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id)
    {
        User? user = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return this.NotFound();

        return this.Ok(user);
    }
}