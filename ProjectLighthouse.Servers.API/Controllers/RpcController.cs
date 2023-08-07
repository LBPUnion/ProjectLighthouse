using LBPUnion.ProjectLighthouse.Configuration;
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
            ApplicationId = ServerConfiguration.Instance.RichPresenceConfiguration.ApplicationId,
            PartyIdPrefix = ServerConfiguration.Instance.RichPresenceConfiguration.PartyIdPrefix,
            UsernameType = ServerConfiguration.Instance.RichPresenceConfiguration.UsernameType,
            Assets = ServerConfiguration.Instance.RichPresenceConfiguration.Assets,
        };
        
        return this.Ok(rpcInformation);
    }
}