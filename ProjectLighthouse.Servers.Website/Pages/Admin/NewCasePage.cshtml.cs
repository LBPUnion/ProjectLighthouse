using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class NewCasePage : BaseLayout
{
    public NewCasePage(Database database) : base(database)
    {}

    public CaseType Type { get; set; }
    public int AffectedId { get; set; }

    public IActionResult OnGet([FromQuery] CaseType? type, [FromQuery] int? affectedId)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.Redirect("/login");

        if (type == null) return this.BadRequest();
        if (affectedId == null) return this.BadRequest();

        this.Type = (CaseType)type;
        this.AffectedId = (int)affectedId;
        
        return this.Page();
    }

    public async Task<IActionResult> OnPost(CaseType? type, string? reason, string? modNotes, DateTime expires, int? affectedId)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.Redirect("/login");

        if (type == null) return this.BadRequest();
        if (affectedId == null) return this.BadRequest();

        reason ??= string.Empty;
        modNotes ??= string.Empty;
        
        // this is fucking ugly
        // if id is invalid then return bad request
        if (!(await ((CaseType)type).IsIdValid((int)affectedId, this.Database))) return this.BadRequest();
        
        ModerationCase @case = new()
        {
            Type = (CaseType)type,
            Reason = reason,
            ModeratorNotes = modNotes,
            ExpiresAt = expires,
            CreatedAt = DateTime.Now,
            CreatorId = user.UserId,
            AffectedId = (int)affectedId,
        };

        this.Database.Cases.Add(@case);
        await this.Database.SaveChangesAsync();
        
        return this.Redirect("/moderation/cases/0");
    }
}