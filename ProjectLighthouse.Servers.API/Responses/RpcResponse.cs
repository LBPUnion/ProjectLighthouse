#nullable disable

using LBPUnion.ProjectLighthouse.Configuration.ConfigurationCategories;

namespace LBPUnion.ProjectLighthouse.Servers.API.Responses;

public class RpcResponse
{
    public long ApplicationId { get; set; }
    public string PartyIdPrefix { get; set; }
    public UsernameType UsernameType { get; set; }
    public RpcAssets Assets { get; set; }
}