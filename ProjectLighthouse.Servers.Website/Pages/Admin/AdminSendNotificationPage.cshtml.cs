using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class AdminSendNotificationPage :  BaseLayout
{
    public AdminSendNotificationPage(DatabaseContext database) : base(database)
    { }

    public UserEntity? TargetedUser;

    public async Task<IActionResult> OnGet([FromRoute] int id)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        this.TargetedUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (this.TargetedUser == null) return this.NotFound();

        return this.Page();
    }

    public async Task<IActionResult> OnPost([FromRoute] int id, [FromForm] string notificationContent)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        this.TargetedUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (this.TargetedUser == null) return this.NotFound();

        await this.Database.SendNotification(this.TargetedUser.UserId, notificationContent);

        return this.Redirect($"/user/{this.TargetedUser.UserId}");
    }
}