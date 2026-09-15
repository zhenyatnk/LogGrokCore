using LogGrokX.Data.Virtualization;

namespace LogGrokX.Data.Index
{
    public interface IIndexedLinesProvider : IItemProvider<int>
    {
        int GetIndexByValue(int value);
    }
}