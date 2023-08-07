#nullable disable

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public class RpcInformation
{
    public long ApplicationId { get; set; }
    public string PartyIdPrefix { get; set; }
    public UsernameType UsernameType { get; set; }
    public RpcAssets Assets { get; set; }
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