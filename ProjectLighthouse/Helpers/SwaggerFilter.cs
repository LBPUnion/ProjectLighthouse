using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LBPUnion.ProjectLighthouse.Helpers;

/// <summary>
/// <para>
/// A filter for the swagger documentation endpoint.
/// </para>
/// <para>
/// Makes sure that only endpoints under <c>/api/v1</c> show up.
/// </para>
/// </summary>
public class SwaggerFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        List<KeyValuePair<string, OpenApiPathItem>> nonApiRoutes = swaggerDoc.Paths.Where(x => !x.Key.ToLower().StartsWith("/api/v1")).ToList();
        nonApiRoutes.ForEach(x => swaggerDoc.Paths.Remove(x.Key));
    }
}