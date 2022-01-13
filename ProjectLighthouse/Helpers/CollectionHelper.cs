using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types.Categories;

namespace LBPUnion.ProjectLighthouse.Helpers
{
    public static class CollectionHelper
    {
        public static readonly List<Category> Categories = new();

        static CollectionHelper()
        {
            Categories.Add(new TeamPicksCategory());
            Categories.Add(new NewestLevelsCategory());
            Categories.Add(new QueueCategory());
            Categories.Add(new HeartedCategory());

            using Database database = new();
            foreach (DatabaseCategory category in database.CustomCategories) Categories.Add(new CustomCategory(category));
        }
    }
}