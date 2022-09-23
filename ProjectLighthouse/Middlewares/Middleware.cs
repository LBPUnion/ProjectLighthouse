using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Middlewares;

public abstract class Middleware
{
    // this makes it consistent with typical middleware usage
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    protected internal RequestDelegate next { get; init; }

    protected Middleware(RequestDelegate next)
    {
        this.next = next;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract Task InvokeAsync(HttpContext ctx);
}