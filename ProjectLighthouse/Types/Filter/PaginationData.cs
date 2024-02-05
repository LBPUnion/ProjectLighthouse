using System;
using LBPUnion.ProjectLighthouse.Configuration;

namespace LBPUnion.ProjectLighthouse.Types.Filter;

public struct PaginationData
{
    public PaginationData()
    { }

    public int PageStart { get; init; } = 0;
    public int PageSize { get; init; } = 0;
    public int TotalElements { get; set; } = 0;
    public int MaxElements { get; set; } = ServerConfiguration.Instance.UserGeneratedContentLimits.EntitledSlots;

    public int HintStart => this.PageStart + Math.Min(this.PageSize, this.MaxElements);
}