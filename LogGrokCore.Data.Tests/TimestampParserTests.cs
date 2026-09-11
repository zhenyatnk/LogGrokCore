using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogGrokCore.Data.Tests;

[TestClass]
public class TimestampParserTests
{
    [TestMethod]
    public void ParsesTimeOnlyWithExplicitFormat()
    {
        var expected = new TimeSpan(0, 12, 34, 56, 789).Ticks;

        var parsed = TimestampParser.TryGetTicks("12:34:56.789", "HH:mm:ss.fff", out var ticks);

        Assert.IsTrue(parsed);
        Assert.AreEqual(expected, ticks);
    }

    [TestMethod]
    public void ParsesFullTimestampWithoutExplicitFormat()
    {
        var expected = new DateTime(2024, 1, 2, 3, 4, 5, 678).Ticks;

        var parsed = TimestampParser.TryGetTicks("2024-01-02 03:04:05.678", null, out var ticks);

        Assert.IsTrue(parsed);
        Assert.AreEqual(expected, ticks);
    }

    [TestMethod]
    public void ReturnsFalseForInvalidInput()
    {
        var parsed = TimestampParser.TryGetTicks("not a timestamp", "HH:mm:ss.fff", out var ticks);

        Assert.IsFalse(parsed);
        Assert.AreEqual(-1L, ticks);
    }

    [TestMethod]
    public void FormatsTimeOnly()
    {
        var ticks = new TimeSpan(0, 12, 34, 56, 789).Ticks;

        Assert.AreEqual("12:34:56.789", TimestampParser.Format(ticks));
    }

    [TestMethod]
    public void FormatsFullTimestamp()
    {
        var ticks = new DateTime(2024, 1, 2, 3, 4, 5, 678).Ticks;

        Assert.AreEqual("2024-01-02 03:04:05.678", TimestampParser.Format(ticks));
    }

    [TestMethod]
    public void FormatsEmptyForMissingTicks()
    {
        Assert.AreEqual(string.Empty, TimestampParser.Format(-1));
    }
}
