using System.Collections.Generic;

namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

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