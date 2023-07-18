using LBPUnion.ProjectLighthouse.Database;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;

public class BaseFrame : BaseLayout
{
    public BaseFrame(DatabaseContext database) : base(database)
    { }
}