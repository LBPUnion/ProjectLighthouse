#nullable enable
using System.IO;
using System.Threading.Tasks;
using Kettu;
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

    public MessageController(Database database)
    {
        this.database = database;
    }

    [HttpGet("eula")]
    public async Task<IActionResult> Eula()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        return this.Ok($"{EulaHelper.License}\n{ServerSettings.Instance.EulaText}");
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

        string announceText = ServerSettings.Instance.AnnounceText;

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

        string loggedText = await new StreamReader(this.Request.Body).ReadToEndAsync();

        Logger.Log($"{user.Username}: {loggedText}", LoggerLevelFilter.Instance);
        return this.Ok(loggedText);
    }
}