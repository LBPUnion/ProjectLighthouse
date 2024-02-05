#nullable disable

using LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public class RpcResponse
{
    public string ApplicationId { get; set; }
    public string PartyIdPrefix { get; set; }
    public UsernameType UsernameType { get; set; }
    public RpcAssets Assets { get; set; }
    
    public static RpcResponse CreateFromConfiguration(RichPresenceConfiguration configuration) =>
        new()
        {
            ApplicationId = configuration.ApplicationId,
            PartyIdPrefix = configuration.PartyIdPrefix,
            UsernameType = configuration.UsernameType,
            Assets = new RpcAssets
            {
                UseApplicationAssets = configuration.Assets.UseApplicationAssets,
                PodAsset = AssetConvertNull(configuration.Assets.PodAsset),
                MoonAsset = AssetConvertNull(configuration.Assets.MoonAsset),
                RemoteMoonAsset = AssetConvertNull(configuration.Assets.RemoteMoonAsset),
                DeveloperAsset = AssetConvertNull(configuration.Assets.DeveloperAsset),
                DeveloperAdventureAsset = AssetConvertNull(configuration.Assets.DeveloperAdventureAsset),
                DlcAsset = AssetConvertNull(configuration.Assets.DlcAsset),
                FallbackAsset = AssetConvertNull(configuration.Assets.FallbackAsset),
            },
        };
    
    private static string AssetConvertNull(string asset) => string.IsNullOrWhiteSpace(asset) ? null : asset;
}