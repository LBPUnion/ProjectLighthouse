using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Database;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Middlewares;

public abstract class MiddlewareDBContext
{
    // this makes it consistent with typical middleware usage
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    protected RequestDelegate next { get; }

    protected MiddlewareDBContext(RequestDelegate next)
    {
        this.next = next;
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract Task InvokeAsync(HttpContext ctx, DatabaseContext db);
}