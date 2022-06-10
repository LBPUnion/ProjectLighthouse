using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Middlewares;

public abstract class Middleware
{
    private protected RequestDelegate next { get; init; }

    protected Middleware(RequestDelegate next)
    {
        this.next = next;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract Task InvokeAsync(HttpContext httpContext);
}