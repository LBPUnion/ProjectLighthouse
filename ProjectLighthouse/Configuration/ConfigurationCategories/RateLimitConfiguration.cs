using System.Collections.Generic;

namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class RateLimitConfiguration
{
    public int RequestsPerInterval { get; set; } = 10;
    public int RequestInterval { get; set; } = 30;
    public Dictionary<string, RateLimitOverride> RateLimitOverrides { get; set; } = new() { { "/upload", new RateLimitOverride() }, };
}