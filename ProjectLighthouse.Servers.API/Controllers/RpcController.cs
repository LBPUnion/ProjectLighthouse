using LBPUnion.ProjectLighthouse.Servers.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

public class RpcController : ApiEndpointController
{
    /// <summary>
    /// Returns basic information that RPC clients can use for self-configuration.
    /// </summary>
    /// <returns>RpcInformation</returns>
    /// <response code="200">The RPC information.</response>
    [HttpGet("rpc")]
    [ProducesResponseType(typeof(RpcInformation), StatusCodes.Status200OK)]
    public IActionResult GetRpc()
    {
        RpcInformation rpcInformation = new()
        {
            ApplicationId = 1060973475151495288,
            PartyIdPrefix = "pl",
            UsernameType = UsernameType.Integer,
            Assets = new RpcAssets
            {
                PodAsset = "9c412649a07a8cb678a2a25214ed981001dd08ca",
                MoonAsset = "a891bbcf9ad3518b80c210813cce8ed292ed4c62",
                RemoteMoonAsset = "a891bbcf9ad3518b80c210813cce8ed292ed4c62",
                DeveloperAsset = "7d3df5ce61ca90a80f600452cd3445b7a775d47e",
                DeveloperAdventureAsset = "7d3df5ce61ca90a80f600452cd3445b7a775d47e",
                DlcAsset = "2976e45d66b183f6d3242eaf01236d231766295f",
                FallbackAsset = "e6bb64f5f280ce07fdcf4c63e25fa8296c73ec29",
            },
        };
        
        return this.Ok(rpcInformation);
    }
}