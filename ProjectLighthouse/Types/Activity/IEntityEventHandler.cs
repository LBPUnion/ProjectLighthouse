using LBPUnion.ProjectLighthouse.Database;

namespace LBPUnion.ProjectLighthouse.Types.Activity;

public interface IEntityEventHandler
{
    public void OnEntityInserted<T>(DatabaseContext database, T entity) where T : class;
    public void OnEntityChanged<T>(DatabaseContext database, T origEntity, T currentEntity) where T : class;
    public void OnEntityDeleted<T>(DatabaseContext database, T entity) where T : class;
}