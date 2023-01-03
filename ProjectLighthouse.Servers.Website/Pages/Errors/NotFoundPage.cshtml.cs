using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Errors;

public class NotFoundPage : BaseLayout
{
    public NotFoundPage(Database database) : base(database)
    {}

    public IActionResult OnGet() => this.Page();
}