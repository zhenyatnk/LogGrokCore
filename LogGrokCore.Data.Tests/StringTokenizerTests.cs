using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogGrokCore.Data.Tests;

[TestClass]
public class StringTokenizerTests
{
    [TestMethod]
    public void SimpleTest()
    {
        var source = "a\r\nb";
        var tokens = source.Tokenize().ToList();
        Assert.AreEqual(2, tokens.Count);
        Assert.AreEqual("a", tokens[0].ToString());
        Assert.AreEqual("b", tokens[1].ToString());
    }
    
    [TestMethod]
    public void TailingCrLf()
    {
        var source = "a\r\nb\r\n";
        var tokens = source.Tokenize().ToList();
        Assert.AreEqual(2, tokens.Count);
        Assert.AreEqual("a", tokens[0].ToString());
        Assert.AreEqual("b", tokens[1].ToString());
    }
}