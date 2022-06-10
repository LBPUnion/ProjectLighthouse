#nullable enable

namespace LBPUnion.ProjectLighthouse.Administration;

public struct AdminPanelStatistic
{
    public AdminPanelStatistic(string statisticNamePlural, int count, string? viewAllEndpoint = null)
    {
        this.StatisticNamePlural = statisticNamePlural;
        this.Count = count;
        this.ViewAllEndpoint = viewAllEndpoint;
    }

    public readonly string StatisticNamePlural;
    public readonly int Count;

    public readonly string? ViewAllEndpoint;
}