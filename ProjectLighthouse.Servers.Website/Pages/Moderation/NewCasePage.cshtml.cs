using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Moderation.Cases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class NewCasePage : BaseLayout
{
    public NewCasePage(DatabaseContext database) : base(database)
    { }

    public CaseType Type { get; set; }

    public int AffectedId { get; set; }
    public UserEntity? AffectedUser { get; set; }
    public List<ModerationCaseEntity> AffectedHistory { get; set; } = new();

    public string? Error { get; private set; }

    public async Task<IActionResult> OnGet([FromQuery] CaseType? type, [FromQuery] int? affectedId)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.Redirect("/login");

        if (type == null) return this.BadRequest();
        if (affectedId == null) return this.BadRequest();

        this.Type = type.Value;

        this.AffectedId = affectedId.Value;
        
        this.AffectedUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == this.AffectedId);
        if (this.AffectedUser == null) return this.BadRequest();
        
        this.AffectedHistory = await this.Database.Cases.Where(c => c.AffectedId == this.AffectedId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

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

        UserEntity? affectedUser =
            await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == affectedId.Value);

        if (affectedUser == null || affectedUser.IsModerator)
        {
            this.Error = this.Translate(ErrorStrings.ActionNoPermission);
            return this.Page();
        }

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