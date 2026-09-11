using System;
using System.Collections.Generic;
using LogGrokCore.Data.Index;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogGrokCore.Data.Tests;

[TestClass]
public class LineRangeIndexedLinesProviderTests
{
    private static readonly int[] Lines = { 2, 5, 7, 10, 11, 20 };

    [TestMethod]
    public void RestrictsFetchToRequestedLineRange()
    {
        var provider = new LineRangeIndexedLinesProvider(new FakeIndexedLinesProvider(Lines), 5, 12);

        Assert.AreEqual(4, provider.Count);
        CollectionAssert.AreEqual(new[] { 5, 7, 10, 11 }, FetchAll(provider, provider.Count));
    }

    [TestMethod]
    public void GetIndexByValueReturnsPositionWithinRange()
    {
        var provider = new LineRangeIndexedLinesProvider(new FakeIndexedLinesProvider(Lines), 5, 12);

        Assert.AreEqual(0, provider.GetIndexByValue(5));
        Assert.AreEqual(2, provider.GetIndexByValue(10));
        Assert.AreEqual(4, provider.GetIndexByValue(100));
        Assert.AreEqual(0, provider.GetIndexByValue(1));
    }

    [TestMethod]
    public void EmptyRangeProducesEmptyProvider()
    {
        var provider = new LineRangeIndexedLinesProvider(new FakeIndexedLinesProvider(Lines), 30, 40);

        Assert.AreEqual(0, provider.Count);
    }

    private static int[] FetchAll(IIndexedLinesProvider provider, int count)
    {
        var result = new int[count];
        provider.Fetch(0, result);
        return result;
    }

    private sealed class FakeIndexedLinesProvider : IIndexedLinesProvider
    {
        private readonly IReadOnlyList<int> _lines;

        public FakeIndexedLinesProvider(IReadOnlyList<int> lines) => _lines = lines;

        public int Count => _lines.Count;

        public void Fetch(int start, Span<int> values)
        {
            for (var i = 0; i < values.Length; i++)
                values[i] = _lines[start + i];
        }

        public int GetIndexByValue(int value)
        {
            for (var i = 0; i < _lines.Count; i++)
            {
                if (_lines[i] >= value)
                    return i;
            }

            return _lines.Count - 1;
        }
    }
}
