using System;

namespace LogGrokX.Data.Virtualization
{
    public interface IItemProvider<T>
    {
        int Count { get; }
        
        void Fetch(int start, Span<T> values);
    }
}
