#nullable enable
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.ExternalAuth;

public class ManageUserApprovedIpAddressesPage : BaseLayout
{
    public List<UserApprovedIpAddress> ApprovedIpAddresses = new();

    public ManageUserApprovedIpAddressesPage(Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        this.ApprovedIpAddresses = await this.Database.UserApprovedIpAddresses.Where(a => a.UserId == user.UserId).ToListAsync();

        return this.Page();
    }
}