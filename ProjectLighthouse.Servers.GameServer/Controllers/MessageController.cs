using System.Text;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types.Entities.Notifications;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Mail;
using LBPUnion.ProjectLighthouse.Types.Serialization;
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
    private readonly DatabaseContext database;

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

    public MessageController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("eula")]
    public IActionResult Eula() => this.Ok($"{license}\n{ServerConfiguration.Instance.EulaText}");

    [HttpGet("announce")]
    public async Task<IActionResult> Announce()
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.BadRequest();

        StringBuilder announceText = new(ServerConfiguration.Instance.AnnounceText);

        announceText.Replace("%user", user.Username);
        announceText.Replace("%id", token.UserId.ToString());

        if (ServerConfiguration.Instance.UserGeneratedContentLimits.ReadOnlyMode)
        {
            announceText.Append(BaseLayoutStrings.ReadOnlyWarn.Translate(LocalizationManager.DefaultLang) + "\n\n");
        }

        if (ServerConfiguration.Instance.EmailEnforcement.EnableEmailEnforcement)
        {
            announceText.Append("\n\n" + BaseLayoutStrings.EmailEnforcementWarnMain.Translate(LocalizationManager.DefaultLang) + "\n\n");

            if (user.EmailAddress == null)
            {
                announceText.Append(BaseLayoutStrings.EmailEnforcementWarnNoEmail.Translate(LocalizationManager.DefaultLang) + "\n\n");
            }
            else if (!user.EmailAddressVerified)
            {
                announceText.Append(BaseLayoutStrings.EmailEnforcementWarnVerifyEmail.Translate(LocalizationManager.DefaultLang) + "\n\n");
            }
        }

        #if DEBUG
        announceText.Append("\n\n---DEBUG INFO---\n" +
                                  $"user.UserId: {token.UserId}\n" +
                                  $"token.GameVersion: {token.GameVersion}\n" +
                                  $"token.TicketHash: {token.TicketHash}\n" +
                                  $"token.ExpiresAt: {token.ExpiresAt.ToString()}\n" +
                                  "---DEBUG INFO---");
        #endif

        return this.Ok(announceText.ToString());
    }

    [HttpGet("notification")]
    [Produces("text/xml")]
    public async Task<IActionResult> Notification()
    {
        GameTokenEntity token = this.GetToken();

        List<NotificationEntity> notifications = await this.database.Notifications
            .Where(n => n.UserId == token.UserId)
            .Where(n => !n.IsDismissed)
            .OrderByDescending(n => n.Id)
            .ToListAsync();

        // We don't need to do any more work if there are no unconverted notifications to begin with.
        if (notifications.Count == 0) return this.Ok();

        StringBuilder builder = new();

        foreach (NotificationEntity notification in notifications)
        {
            builder.AppendLine(LighthouseSerializer.Serialize(this.HttpContext.RequestServices,
                GameNotification.CreateFromEntity(notification)));

            notification.IsDismissed = true;
        }

        await this.database.SaveChangesAsync();

        return this.Ok(new LbpCustomXml
        {
            Content = builder.ToString(),
        });
    }

    /// <summary>
    ///     Filters chat messages sent by a user.
    ///     The response sent is the text that will appear in-game.
    /// </summary>
    [HttpPost("filter")]
    public async Task<IActionResult> Filter(IMailService mailService)
    {
        GameTokenEntity token = this.GetToken();

        string message = await this.ReadBodyAsync();

        const int lbpCharLimit = 512;
        if (message.Length > lbpCharLimit) return this.BadRequest();

        if (message.StartsWith("/setemail ") && ServerConfiguration.Instance.Mail.MailEnabled)
        {
            string email = message[(message.IndexOf(" ", StringComparison.Ordinal)+1)..];

            // Return a bad request on invalid email address
            if (!SMTPHelper.IsValidEmail(this.database, email)) return this.BadRequest();

            UserEntity? user = await this.database.UserFromGameToken(token);
            if (user == null || user.EmailAddressVerified) return this.BadRequest();

            user.EmailAddress = email;
            await SMTPHelper.SendVerificationEmail(this.database, mailService, user);

            return this.Ok();
        }

        string username = await this.database.UsernameFromGameToken(token);

        if (ServerConfiguration.Instance.LogChatMessages) Logger.Info($"{username}: \"{message}\"", LogArea.Filter);

        message = CensorHelper.FilterMessage(message, FilterLocation.ChatMessage, username);

        return this.Ok(message);
    }
}