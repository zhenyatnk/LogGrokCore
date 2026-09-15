using System.Collections.Generic;

namespace LogGrokX
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Yield<T>(this T source) 
        {
            yield return source;
        }
    }
}