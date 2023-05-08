using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Filters;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Extensions;

public static class ControllerExtensions
{
    private static GameVersion GetGameFilter(string? gameFilterType, GameVersion version)
    {
        return version switch
        {
            GameVersion.LittleBigPlanetVita => GameVersion.LittleBigPlanetVita,
            GameVersion.LittleBigPlanetPSP => GameVersion.LittleBigPlanetPSP,
            _ => gameFilterType switch
            {
                "lbp1" => GameVersion.LittleBigPlanet1,
                "lbp2" => GameVersion.LittleBigPlanet2,
                "lbp3" => GameVersion.LittleBigPlanet3,
                "both" => GameVersion.LittleBigPlanet2, // LBP2 default option
                null => GameVersion.LittleBigPlanet1,
                _ => GameVersion.Unknown,
            },
        };
    }

    public static SlotQueryBuilder FilterFromRequest(this ControllerBase controller, GameTokenEntity token)
    {
        SlotQueryBuilder queryBuilder = new();

        List<string> authorLabels = new();
        for (int i = 0; i < 3; i++)
        {
            string? label = controller.Request.Query[$"labelFilter{i}"];
            if (label == null) continue;
            authorLabels.Add(label);
        }

        if (authorLabels.Count > 0) queryBuilder.AddFilter(new AuthorLabelFilter(authorLabels.ToArray()));

        if (bool.TryParse(controller.Request.Query["move"], out bool movePack) && !movePack)
            queryBuilder.AddFilter(new ExcludeMovePackFilter());

        if (bool.TryParse(controller.Request.Query["crosscontrol"], out bool crossControl) && crossControl)
            queryBuilder.AddFilter(new CrossControlFilter());

        if (int.TryParse(controller.Request.Query["players"], out int minPlayers) && minPlayers >= 1)
            queryBuilder.AddFilter(new PlayerCountFilter(minPlayers));

        if (controller.Request.Query.ContainsKey("dateFilterType"))
        {
            string dateFilter = (string?)controller.Request.Query["dateFilterType"] ?? "";
            long oldestTime = dateFilter switch
            {
                "thisWeek" => DateTimeOffset.Now.AddDays(-7).ToUnixTimeMilliseconds(),
                "thisMonth" => DateTimeOffset.Now.AddDays(-31).ToUnixTimeMilliseconds(),
                _ => 0,
            };
            if (oldestTime != 0) queryBuilder.AddFilter(new FirstUploadedFilter(oldestTime));
        }

        if (controller.Request.Query.ContainsKey("textFilter"))
        {
            string textFilter = (string?)controller.Request.Query["textFilter"] ?? "";

            if (!string.IsNullOrWhiteSpace(textFilter)) queryBuilder.AddFilter(new TextFilter(textFilter));
        }

        GameVersion targetVersion = token.GameVersion;

        if (targetVersion != GameVersion.LittleBigPlanet1) queryBuilder.AddFilter(new ExcludeLBP1OnlyFilter(token.UserId));

        if (controller.Request.Query.ContainsKey("gameFilterType"))
        {
            string gameFilter = (string?)controller.Request.Query["gameFilterType"] ?? "";
            GameVersion filterVersion = GetGameFilter(gameFilter, targetVersion);
            // Don't serve lbp3 levels to lbp2 just cause of the game filter
            if (filterVersion <= targetVersion) targetVersion = filterVersion;
        }

        queryBuilder.AddFilter(new SubLevelFilter());
        queryBuilder.AddFilter(new HiddenSlotFilter());
        queryBuilder.AddFilter(new SlotTypeFilter(SlotType.User));
        queryBuilder.AddFilter(new GameVersionFilter(targetVersion));

        return queryBuilder;
    }
}