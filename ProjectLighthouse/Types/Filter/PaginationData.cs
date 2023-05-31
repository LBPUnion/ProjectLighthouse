using System;

namespace LBPUnion.ProjectLighthouse.Types.Filter;

public struct PaginationData
{
    public PaginationData()
    { }

    public int PageStart { get; init; } = 0;
    public int PageSize { get; init; } = 0;
    public int TotalElements { get; set; } = 0;
    public int MaxElements { get; set; } = 30;

    public int HintStart => this.PageStart + Math.Min(this.PageSize, this.MaxElements);
}