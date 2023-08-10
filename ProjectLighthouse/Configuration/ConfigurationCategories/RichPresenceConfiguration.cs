using JetBrains.Annotations;

namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

// ReSharper disable UnusedMember.Global

public class RichPresenceConfiguration
{
    public long ApplicationId { get; [UsedImplicitly] set; } = 1060973475151495288;
    public string PartyIdPrefix { get; [UsedImplicitly] set; } = "project-lighthouse";
    public UsernameType UsernameType { get; [UsedImplicitly] set; } = UsernameType.Integer;
    public RpcAssets Assets { get; [UsedImplicitly] set; } = new();
}

public class RpcAssets
{
    public string PodAsset { get; set; }
    public string MoonAsset { get; set; }
    public string RemoteMoonAsset { get; set; }
    public string DeveloperAsset { get; set; }
    public string DeveloperAdventureAsset { get; set; }
    public string DlcAsset { get; set; }
    public string FallbackAsset { get; set; }
}

public enum UsernameType
{
    Integer = 0,
    Username = 1,
}