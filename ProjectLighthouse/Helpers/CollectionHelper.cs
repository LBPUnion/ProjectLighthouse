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
            Categories.Add
            (
                new CustomCategory
                (
                    "Custom Category",
                    "This is a custom category!",
                    "custom1",
                    "g820614",
                    new[]
                    {
                        35, 37, 2979, 3042,
                    }
                )
            );
        }
    }
}