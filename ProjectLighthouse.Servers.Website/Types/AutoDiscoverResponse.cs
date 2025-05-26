using Newtonsoft.Json;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Types;

public record AutoDiscoverResponse
{
    [JsonProperty("version")]
    public uint Version { get; set; }
    [JsonProperty("serverBrand")]
    public string ServerBrand { get; set; }
    [JsonProperty("serverDescription")]
    public string ServerDescription { get; set; }
    [JsonProperty("url")]
    public string Url { get; set; }
    [JsonProperty("bannerImageUrl")]
    public string? BannerImageUrl { get; set; }
    [JsonProperty("usesCustomDigestKey")]
    public bool UsesCustomDigestKey { get; set; }
}
