using Newtonsoft.Json;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Types;

public record AutoDiscoverResponse
{
    [JsonProperty("version")]
    public required uint Version { get; set; }
    [JsonProperty("serverBrand")]
    public required string ServerBrand { get; set; }
    [JsonProperty("serverDescription")]
    public required string ServerDescription { get; set; }
    [JsonProperty("url")]
    public required string Url { get; set; }
    [JsonProperty("bannerImageUrl")]
    public string? BannerImageUrl { get; set; }
    [JsonProperty("usesCustomDigestKey")]
    public required bool UsesCustomDigestKey { get; set; }
}
