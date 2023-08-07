using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Servers.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

public class RpcController : ApiEndpointController
{
    /// <summary>
    /// Returns basic information that Discord RPC clients can use for self-configuration.
    /// </summary>
    /// <returns>RpcResponse</returns>
    /// <response code="200">The RPC configuration.</response>
    [HttpGet("rpc")]
    [ProducesResponseType(typeof(RpcResponse), StatusCodes.Status200OK)]
    public IActionResult GetRpcConfiguration()
    {
        RpcResponse rpcResponse = new()
        {
            ApplicationId = ServerConfiguration.Instance.RichPresenceConfiguration.ApplicationId,
            PartyIdPrefix = ServerConfiguration.Instance.RichPresenceConfiguration.PartyIdPrefix,
            UsernameType = ServerConfiguration.Instance.RichPresenceConfiguration.UsernameType,
            Assets = ServerConfiguration.Instance.RichPresenceConfiguration.Assets,
        };
        
        return this.Ok(rpcResponse);
    }
}