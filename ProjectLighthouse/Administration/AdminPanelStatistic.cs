#nullable enable

namespace LBPUnion.ProjectLighthouse.Administration;

public struct AdminPanelStatistic
{
    public AdminPanelStatistic(string statisticNamePlural, int count, string? viewAllEndpoint = null, int? secondStatistic = null)
    {
        this.StatisticNamePlural = statisticNamePlural;
        this.Count = count;
        this.SecondStatistic = secondStatistic;
        this.ViewAllEndpoint = viewAllEndpoint;
    }

    public readonly string StatisticNamePlural;
    public readonly int Count;
    public readonly int? SecondStatistic;

    public readonly string? ViewAllEndpoint;
}