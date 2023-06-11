#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.API.Responses;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

/// <summary>
/// A collection of endpoints relating to users.
/// </summary>
public class UserEndpoints : ApiEndpointController
{
    private readonly DatabaseContext database;

    public UserEndpoints(DatabaseContext database)
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
    [ProducesResponseType(typeof(ApiUser), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id)
    {
        UserEntity? user = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (user == null) return this.NotFound();

        return this.Ok(ApiUser.CreateFromEntity(user));
    }

    [HttpGet("username/{username}")]
    [ProducesResponseType(typeof(ApiUser), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(string username)
    {
        UserEntity? user = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return this.NotFound();

        return this.Ok(ApiUser.CreateFromEntity(user));
    }

    /// <summary>
    /// Searches for the user based on the query
    /// </summary>
    /// <param name="query">The search query</param>
    /// <returns>A list of users</returns>
    /// <response code="200">The list of users, if any were found</response>
    /// <response code="404">No users matched the query</response>
    [HttpGet("search/user")]
    [ProducesResponseType(typeof(ApiUser), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SearchUsers(string query)
    {
        List<ApiUser> users = (await this.database.Users
            .Where(u => u.PermissionLevel != PermissionLevel.Banned && u.Username.Contains(query))
            .Where(u => u.ProfileVisibility == PrivacyType.All) // TODO: change check for when user is logged in
            .OrderByDescending(b => b.UserId)
            .Take(20)
            .ToListAsync()).ToSerializableList(ApiUser.CreateFromEntity);
        if (!users.Any()) return this.NotFound();

        return this.Ok(users);
    }

    /// <summary>
    /// Gets a user and their information from the database.
    /// </summary>
    /// <param name="id">The ID of the user</param>
    /// <returns>The user's status</returns>
    /// <response code="200">The user's status, if successful.</response>
    /// <response code="404">The user could not be found.</response>
    [HttpGet("user/{id:int}/status")]
    [ProducesResponseType(typeof(ApiUser), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUserStatus(int id)
    {
        UserStatus userStatus = new(this.database, id);

        return this.Ok(userStatus);
    }

    [HttpPost("user/inviteToken")]
    [HttpPost("user/inviteToken/{username}")]
    public async Task<IActionResult> CreateUserInviteToken([FromRoute] string? username)
    {
        if (!Configuration.ServerConfiguration.Instance.Authentication.RegistrationEnabled)
            return this.NotFound();

        string? authHeader = this.Request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(authHeader)) return this.NotFound();

        string authToken = authHeader[(authHeader.IndexOf(' ') + 1)..];

        ApiKeyEntity? apiKey = await this.database.APIKeys.FirstOrDefaultAsync(k => k.Key == authToken);
        if (apiKey == null) return this.StatusCode(403);

        if (!string.IsNullOrWhiteSpace(username))
        {
            bool userExists = await this.database.Users.AnyAsync(u => u.Username == username);
            if (userExists) return this.BadRequest();
        }

        RegistrationTokenEntity token = new()
        {
            Created = DateTime.Now,
            Token = CryptoHelper.GenerateAuthToken(),
            Username = username,
        };

        this.database.RegistrationTokens.Add(token);
        await this.database.SaveChangesAsync();

        return this.Ok(token.Token);
    }
}