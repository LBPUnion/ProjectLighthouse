namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: NewsPost
/// </summary>
public class NewsActivityEntity : ActivityEntity
{
    public string Title { get; set; } = "";

    public string Body { get; set; } = "";
}