#nullable enable
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// ReSharper disable RouteTemplates.ActionRoutePrefixCanBeExtractedToControllerRoute

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

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

    /// <summary>
    /// Gets a user and their information from the database.
    /// </summary>
    /// <param name="id">The ID of the user</param>
    /// <returns>The user's status</returns>
    /// <response code="200">The user's status, if successful.</response>
    /// <response code="404">The user could not be found.</response>
    [HttpGet("user/{id:int}/status")]
    [ProducesResponseType(typeof(UserStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUserStatus(int id)
    {
        UserStatus userStatus = new(this.database, id);

        return this.Ok(userStatus);
    }

    [HttpPost("user/inviteToken")]
    public async Task<IActionResult> CreateUserInviteToken()
    {
        if (!Configuration.ServerConfiguration.Instance.Authentication.PrivateRegistration &&
            !Configuration.ServerConfiguration.Instance.Authentication.RegistrationEnabled)
            return this.NotFound();

        string authHeader = this.Request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(authHeader)) return this.NotFound();

        string authToken = authHeader[(authHeader.IndexOf(' ') + 1)..];

        APIKey? apiKey = await this.database.APIKeys.FirstOrDefaultAsync(k => k.Key == authToken);
        if (apiKey == null) return this.StatusCode(403, null);

        RegistrationToken token = new()
        {
            Created = DateTime.Now,
            Token = CryptoHelper.GenerateAuthToken(),
        };

        this.database.RegistrationTokens.Add(token);
        await this.database.SaveChangesAsync();

        return this.Ok(token.Token);
    }
}