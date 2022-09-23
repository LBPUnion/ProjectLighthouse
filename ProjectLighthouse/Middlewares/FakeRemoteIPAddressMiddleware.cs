using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Middlewares;

public class FakeRemoteIPAddressMiddleware : Middleware
{
    private readonly IPAddress fakeIpAddress = IPAddress.Parse("127.0.0.1");

    public FakeRemoteIPAddressMiddleware(RequestDelegate next) : base(next)
    {}

    public override async Task InvokeAsync(HttpContext ctx)
    {
        ctx.Connection.RemoteIpAddress = this.fakeIpAddress;

        await this.next(ctx);
    }
}