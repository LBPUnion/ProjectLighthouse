#nullable enable
using System.IO;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi;

[ApiController]
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
    public async Task<IActionResult> Eula()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        return this.Ok($"{license}\n{ServerConfiguration.Instance.EulaText}");
    }

    [HttpGet("announce")]
    public async Task<IActionResult> Announce()
    {
        #if !DEBUG
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");
        #else
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;
        #endif

        string announceText = ServerConfiguration.Instance.AnnounceText;

        announceText = announceText.Replace("%user", user.Username);
        announceText = announceText.Replace("%id", user.UserId.ToString());

        return this.Ok
        (
            announceText +
            #if DEBUG
            "\n\n---DEBUG INFO---\n" +
            $"user.UserId: {user.UserId}\n" +
            $"token.Approved: {gameToken.Approved}\n" +
            $"token.Used: {gameToken.Used}\n" +
            $"token.UserLocation: {gameToken.UserLocation}\n" +
            $"token.GameVersion: {gameToken.GameVersion}\n" +
            "---DEBUG INFO---" +
            #endif
            "\n"
        );
    }

    [HttpGet("notification")]
    public IActionResult Notification() => this.Ok();
    /// <summary>
    ///     Filters chat messages sent by a user.
    ///     The reponse sent is the text that will appear in-game.
    /// </summary>
    [HttpPost("filter")]
    public async Task<IActionResult> Filter()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);

        if (user == null) return this.StatusCode(403, "");

        string response = await new StreamReader(this.Request.Body).ReadToEndAsync();

        string scannedText = CensorHelper.ScanMessage(response);

        Logger.LogInfo($"{user.Username}: {response} / {scannedText}", LogArea.Filter);

        return this.Ok(scannedText);
    }
}