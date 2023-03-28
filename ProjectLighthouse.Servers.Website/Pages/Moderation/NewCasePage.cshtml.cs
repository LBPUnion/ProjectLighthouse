using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Moderation.Cases;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class NewCasePage : BaseLayout
{
    public NewCasePage(DatabaseContext database) : base(database)
    {}

    public CaseType Type { get; set; }
    public int AffectedId { get; set; }

    public IActionResult OnGet([FromQuery] CaseType? type, [FromQuery] int? affectedId)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.Redirect("/login");

        if (type == null) return this.BadRequest();
        if (affectedId == null) return this.BadRequest();

        this.Type = type.Value;
        this.AffectedId = affectedId.Value;
        
        return this.Page();
    }

    public async Task<IActionResult> OnPost(CaseType? type, string? reason, string? modNotes, DateTime expires, int? affectedId)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.Redirect("/login");

        if (type == null) return this.BadRequest();
        if (affectedId == null) return this.BadRequest();

        reason ??= string.Empty;
        modNotes ??= string.Empty;
        
        // if id is invalid then return bad request
        if (!await type.Value.IsIdValid((int)affectedId, this.Database)) return this.BadRequest();
        
        ModerationCaseEntity @case = new()
        {
            Type = type.Value,
            Reason = reason,
            ModeratorNotes = modNotes,
            ExpiresAt = expires,
            CreatedAt = DateTime.Now,
            CreatorId = user.UserId,
            CreatorUsername = user.Username,
            AffectedId = affectedId.Value,
        };

        this.Database.Cases.Add(@case);
        await this.Database.SaveChangesAsync();
        
        return this.Redirect("/moderation/cases/0");
    }
}