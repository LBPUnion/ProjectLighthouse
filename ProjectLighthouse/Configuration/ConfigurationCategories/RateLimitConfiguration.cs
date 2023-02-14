using System.Collections.Generic;

namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

public class RateLimitOptions
{
    public bool Enabled;
    public int RequestsPerInterval { get; set; } = 5;
    public int RequestInterval { get; set; } = 15;
}

public class RateLimitConfiguration
{
    public RateLimitOptions GlobalOptions { get; set; } = new();

    public Dictionary<string, RateLimitOptions> OverrideOptions { get; set; } = new()
    {
        {
            "/example/*/wildcard", new RateLimitOptions
            {
                RequestInterval = 5,
                RequestsPerInterval = 10,
                Enabled = true,
            }
        },
    };
}