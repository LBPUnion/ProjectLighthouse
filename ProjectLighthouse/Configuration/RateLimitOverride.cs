namespace LBPUnion.ProjectLighthouse.Configuration;

public class RateLimitOverride
{
    public int RequestsPerInterval { get; set; } = 5;
    public int RequestInterval { get; set; } = 15;
}