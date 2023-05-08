namespace LBPUnion.ProjectLighthouse.Types.Filter;

public struct PaginationData
{
    public PaginationData()
    { }

    public int PageStart { get; set; } = 0;
    public int PageSize { get; set; } = 0;
    public int MaxElements { get; set; } = -1;
}