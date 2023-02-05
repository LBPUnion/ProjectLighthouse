#nullable enable
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class AdminSetGrantedSlotsPage : BaseLayout
{
    public AdminSetGrantedSlotsPage(Database database) : base(database)
    {}

    public User? TargetedUser;

    public async Task<IActionResult> OnGet([FromRoute] int id)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        this.TargetedUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (this.TargetedUser == null) return this.NotFound();

        return this.Page();
    }

    public async Task<IActionResult> OnPost([FromRoute] int id, int grantedSlotCount)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        this.TargetedUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (this.TargetedUser == null) return this.NotFound();

        this.TargetedUser.AdminGrantedSlots = grantedSlotCount;

        await this.Database.SaveChangesAsync();
        return this.Redirect($"/user/{this.TargetedUser.UserId}");
    }
}