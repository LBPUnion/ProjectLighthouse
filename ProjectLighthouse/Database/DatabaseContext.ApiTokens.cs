#nullable enable
using System.Linq;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Http;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    public ApiKeyEntity? ApiKeyFromWebRequest(HttpRequest request)
    {
        string? authHeader = request.Headers["Authorization"];
        if (string.IsNullOrWhiteSpace(authHeader)) return null;

        string authToken = authHeader[(authHeader.IndexOf(' ') + 1)..];

        ApiKeyEntity? apiKey = this.APIKeys.FirstOrDefault(k => k.Key == authToken);
        return apiKey ?? null;
    }
}