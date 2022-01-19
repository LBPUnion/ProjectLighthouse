#nullable enable
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages.Admin;

public class AdminBanUserPage : BaseLayout
{

    public User? TargetedUser;
    public AdminBanUserPage(Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int id)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        this.TargetedUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (this.TargetedUser == null) return this.NotFound();

        return this.Page();
    }

    public async Task<IActionResult> OnPost([FromRoute] int id, string reason)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        this.TargetedUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (this.TargetedUser == null) return this.NotFound();

        this.TargetedUser.Banned = true;
        this.TargetedUser.BannedReason = reason;

        // invalidate all currently active gametokens
        this.Database.GameTokens.RemoveRange(this.Database.GameTokens.Where(t => t.UserId == this.TargetedUser.UserId));

        // invalidate all currently active webtokens
        this.Database.WebTokens.RemoveRange(this.Database.WebTokens.Where(t => t.UserId == this.TargetedUser.UserId));

        await this.Database.SaveChangesAsync();
        return this.Redirect($"/user/{this.TargetedUser.UserId}");
    }
}