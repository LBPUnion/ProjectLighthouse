using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Errors;

public class NotFoundPage : BaseLayout
{
    public NotFoundPage(DatabaseContext database) : base(database)
    {}

    public IActionResult OnGet() => this.Page();
}