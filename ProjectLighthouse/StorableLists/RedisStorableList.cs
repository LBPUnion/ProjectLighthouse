using System.Threading.Tasks;
using Redis.OM.Searching;

namespace LBPUnion.ProjectLighthouse.StorableLists;

public class RedisStorableList<T> : StorableList<T>
{
    private readonly IRedisCollection<T> redisNormalCollection;
    public RedisStorableList(IRedisCollection<T> normalCollection) : base(normalCollection)
    {
        this.redisNormalCollection = normalCollection;
    }
    
    public override Task AddAsync(T item) => this.redisNormalCollection.InsertAsync(item);
    public override void Add(T item)
    {
        this.redisNormalCollection.Insert(item);
    }
    
    public override Task RemoveAsync(T item) => this.redisNormalCollection.DeleteAsync(item);
    public override void Remove(T item)
    {
        this.redisNormalCollection.Delete(item);
    }
    
    public override Task UpdateAsync(T item) => this.redisNormalCollection.UpdateAsync(item);
    public override void Update(T item)
    {
        this.redisNormalCollection.Update(item);
    }
}