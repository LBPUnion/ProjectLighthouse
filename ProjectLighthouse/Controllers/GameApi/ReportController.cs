#nullable enable
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Reports;
using Microsoft.AspNetCore.Mvc;
using LBPUnion.ProjectLighthouse.Helpers;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ReportController : ControllerBase
{
    private readonly Database database;

    public ReportController(Database database)
    {
        this.database = database;
    }

    [HttpPost("grief")]
    public async Task<IActionResult> Report()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(GriefReport));
        GriefReport? report = (GriefReport?) serializer.Deserialize(new StringReader(bodyString));

        if (report == null) return this.BadRequest();

        SanitizationHelper.SanitizeStringsInClass(report);

        report.Bounds = JsonSerializer.Serialize(report.XmlBounds.Rect, typeof(Rectangle));
        report.Players = JsonSerializer.Serialize(report.XmlPlayers, typeof(ReportPlayer[]));
        report.Timestamp = TimeHelper.UnixTimeMilliseconds();
        report.ReportingPlayerId = user.UserId;

        this.database.Reports.Add(report);
        await this.database.SaveChangesAsync();

        return this.Ok();
    }

}