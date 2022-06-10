using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.StorableLists;

public class NormalStorableList<T> : StorableList<T>
{
    private readonly List<T> list;
    public NormalStorableList(List<T> normalCollection) : base(normalCollection)
    {
        this.list = normalCollection;
    }
    
    public override void Add(T item)
    {
        this.list.Add(item);
    }
    
    public override Task AddAsync(T item)
    {
        this.list.Add(item);
        return Task.CompletedTask;
    }
    
    public override void RemoveAll(Predicate<T> predicate)
    {
        this.list.RemoveAll(predicate);
    }

    public override Task RemoveAllAsync(Predicate<T> predicate)
    {
        this.list.RemoveAll(predicate);
        return Task.CompletedTask;
    }

    public override Task RemoveAllAsync()
    {
        this.list.RemoveAll(_ => true);
        return Task.CompletedTask;
    }

    public override void RemoveAll()
    {
        this.list.RemoveAll(_ => true);
    }

    public override void Remove(T item)
    {
        this.list.Remove(item);
    }
    
    public override Task RemoveAsync(T item)
    {
        this.list.Remove(item);
        return Task.CompletedTask;
    }
    public override void Update(T item) {}
    public override Task UpdateAsync(T item) => Task.CompletedTask;
}