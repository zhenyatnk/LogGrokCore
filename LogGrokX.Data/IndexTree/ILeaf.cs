using System.Collections.Generic;

namespace LogGrokX.Data.IndexTree
{
    public interface ILeaf<T, out TLeaf> : IEnumerable<T>
        where TLeaf : class
    {
        TLeaf? Add(T value, int valueIndex);

        T this[int index] { get; }

        int Count { get; }
        
        TLeaf? Next { get; }
    }
}