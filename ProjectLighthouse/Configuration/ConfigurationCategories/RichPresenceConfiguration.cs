using System;

namespace LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

// ReSharper disable UnusedMember.Global

[Serializable]
public class RichPresenceConfiguration
{
    public string ApplicationId { get; set; } = "1060973475151495288";
    public string PartyIdPrefix { get; set; } = "project-lighthouse";
    public UsernameType UsernameType { get; set; } = UsernameType.Integer;
    public RpcAssets Assets { get; set; } = new();
}

public class RpcAssets
{
    public bool UseApplicationAssets { get; init; }
    public string PodAsset { get; init; }
    public string MoonAsset { get; init; }
    public string RemoteMoonAsset { get; init; }
    public string DeveloperAsset { get; init; }
    public string DeveloperAdventureAsset { get; init; }
    public string DlcAsset { get; init; }
    public string FallbackAsset { get; init; }
}

public enum UsernameType
{
    Integer = 0,
    Username = 1,
}