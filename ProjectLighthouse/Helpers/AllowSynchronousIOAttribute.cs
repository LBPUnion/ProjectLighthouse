using System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    // Yoinked from https://stackoverflow.com/a/68530667
    // Thanks to T-moty!
    /// <summary>
    ///     Allows synchronous stream operations for this request.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowSynchronousIoAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            IHttpBodyControlFeature syncIoFeature = context.HttpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIoFeature != null) syncIoFeature.AllowSynchronousIO = true;
        }
    }
}