#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/plain")]
public class MessageController : ControllerBase
{
    private readonly Database database;

    private const string license = @"
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.";

    public MessageController(Database database)
    {
        this.database = database;
    }

    [HttpGet("eula")]
    public IActionResult Eula() => this.Ok($"{license}\n{ServerConfiguration.Instance.EulaText}");

    [HttpGet("announce")]
    public async Task<IActionResult> Announce()
    {
        GameToken token = this.GetToken();

        string username = await this.database.UsernameFromGameToken(token);

        string announceText = ServerConfiguration.Instance.AnnounceText;

        announceText = announceText.Replace("%user", username);
        announceText = announceText.Replace("%id", token.UserId.ToString());

        return this.Ok
        (
            announceText +
            #if DEBUG
            "\n\n---DEBUG INFO---\n" +
            $"user.UserId: {token.UserId}\n" +
            $"token.UserLocation: {token.UserLocation}\n" +
            $"token.GameVersion: {token.GameVersion}\n" +
            $"token.TicketHash: {token.TicketHash}\n" +
            $"token.ExpiresAt: {token.ExpiresAt.ToString()}\n" +
            "---DEBUG INFO---" +
            #endif
            (string.IsNullOrWhiteSpace(announceText) ? "" : "\n")
        );
    }

    [HttpGet("notification")]
    public IActionResult Notification() => this.Ok();

    /// <summary>
    ///     Filters chat messages sent by a user.
    ///     The response sent is the text that will appear in-game.
    /// </summary>
    [HttpPost("filter")]
    public async Task<IActionResult> Filter()
    {
        GameToken token = this.GetToken();

        string message = await new StreamReader(this.Request.Body).ReadToEndAsync();

        if (message.StartsWith("/setemail"))
        {
            string email = message[(message.IndexOf(" ", StringComparison.Ordinal)+1)..];
            if (!SanitizationHelper.IsValidEmail(email)) return this.Ok();

            if (await this.database.Users.AnyAsync(u => u.EmailAddress == email)) return this.Ok();

            User? user = await this.database.UserFromGameToken(token);
            if (user == null || user.EmailAddress != null) return this.Ok();

            user.EmailAddress = email;
            user.EmailAddressVerified = true;
            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        string scannedText = CensorHelper.ScanMessage(message);

        string username = await this.database.UsernameFromGameToken(token);

        Logger.Info($"{username}: {message} / {scannedText}", LogArea.Filter);

        return this.Ok(scannedText);
    }
}