namespace LBPUnion.ProjectLighthouse.Configuration;

public class RateLimitOptions
{
    public bool Enabled = true;
    public int RequestsPerInterval { get; set; } = 5;
    public int RequestInterval { get; set; } = 15;
}