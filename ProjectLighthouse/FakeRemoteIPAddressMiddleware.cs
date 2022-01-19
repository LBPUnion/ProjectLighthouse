using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse;

public class FakeRemoteIPAddressMiddleware
{
    private readonly IPAddress fakeIpAddress = IPAddress.Parse("127.0.0.1");
    private readonly RequestDelegate next;

    public FakeRemoteIPAddressMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        httpContext.Connection.RemoteIpAddress = this.fakeIpAddress;

        await this.next(httpContext);
    }
}