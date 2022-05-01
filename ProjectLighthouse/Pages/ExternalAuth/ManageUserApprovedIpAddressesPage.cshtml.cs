#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages.ExternalAuth;

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