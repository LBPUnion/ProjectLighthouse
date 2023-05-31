#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

public abstract class Category
{
    public abstract string Name { get; set; }

    public abstract string Description { get; set; }

    public abstract string IconHash { get; set; }

    public abstract string Endpoint { get; set; }

    public string[] Sorts { get; } = { "relevance", "likes", "plays", "hearts", "date", };

    public abstract string[] Types { get; }

    public abstract string Tag { get; }

    public string IngameEndpoint => $"/searches/{this.Endpoint}";

    public virtual Task<GameCategory> Serialize(DatabaseContext database, GameTokenEntity token, SlotQueryBuilder queryBuilder, int numResults = 1) =>
        Task.FromResult(GameCategory.CreateFromEntity(this, new GenericSerializableList(new List<ILbpSerializable>(), 0, 0)));
}