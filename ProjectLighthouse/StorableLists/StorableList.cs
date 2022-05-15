using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LBPUnion.ProjectLighthouse.StorableLists;

public abstract class StorableList<T> : IEnumerable<T>
{
    private protected readonly IEnumerable<T> NormalCollection;
    protected StorableList(IEnumerable<T> normalCollection)
    {
        this.NormalCollection = normalCollection;
    }

    public abstract void Add(T item);
    public abstract Task AddAsync(T item);
    public abstract void Remove(T item);
    public abstract Task RemoveAsync(T item);
    public abstract void Update(T item);
    public abstract Task UpdateAsync(T item);

    public virtual void RemoveAll(Predicate<T> predicate)
    {
        foreach (T item in this.NormalCollection)
        {
            if (!predicate.Invoke(item)) continue;

            this.Remove(item);
        }
    }

    public virtual async Task RemoveAllAsync(Predicate<T> predicate)
    {
        foreach (T item in this.NormalCollection)
        {
            if (!predicate.Invoke(item)) continue;

            await this.RemoveAsync(item);
        }
    }

    public virtual void RemoveAll()
    {
        foreach (T item in this.NormalCollection)
        {
            this.Remove(item);
        }
    }

    public virtual async Task RemoveAllAsync()
    {
        foreach (T item in this.NormalCollection)
        {
            await this.RemoveAsync(item);
        }
    }

    public IEnumerator<T> GetEnumerator() => this.NormalCollection.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}