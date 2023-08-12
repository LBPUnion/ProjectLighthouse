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