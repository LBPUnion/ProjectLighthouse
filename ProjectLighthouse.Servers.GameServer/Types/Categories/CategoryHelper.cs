using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Configuration;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Categories;

public static class CategoryHelper
{
    public static readonly List<Category> Categories = new();

    static CategoryHelper()
    {
        Dictionary<string, Func<Category>> availableCategories = new()
        {
            ["recently_played"] = () => new RecentlyPlayedCategory(),
            ["recommended"] = () => new RecommendedCategory(),
            ["team_picks"] = () => new TeamPicksCategory(),
            ["most_hearted"] = () => new MostHeartedCategory(),
            ["newest"] = () => new NewestLevelsCategory(),
            ["busiest"] = () => new BusiestCategory(),
            ["most_played"] = () => new MostPlayedCategory(),
            ["my_playlists"] = () => new MyPlaylistsCategory(),
            ["favourite_creators"] = () => new MyHeartedCreatorsCategory(),
            ["queue"] = () => new QueueCategory(),
            ["hearted_levels"] = () => new HeartedCategory(),
            ["highest_rated"] = () => new HighestRatedCategory(),
            ["lucky_dip"] = () => new LuckyDipCategory(),
        };

        foreach (string categoryName in CategoryConfiguration.Instance.Categories)
        {
            if (availableCategories.TryGetValue(categoryName, out Func<Category>? categoryCreator))
                Categories.Add(categoryCreator());
        }

        Categories.Add(new TextSearchCategory());
        using DatabaseContext database = DatabaseContext.CreateNewInstance();
        foreach (DatabaseCategoryEntity category in database.CustomCategories)
            Categories.Add(new CustomCategory(category));
    }
}
