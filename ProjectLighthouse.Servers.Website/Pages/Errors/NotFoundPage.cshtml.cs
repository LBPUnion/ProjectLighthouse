using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Errors;

public class NotFoundPage : BaseLayout
{
    public NotFoundPage(Database database) : base(database)
    {}
    
    public void OnGet()
    {
        
    }
}