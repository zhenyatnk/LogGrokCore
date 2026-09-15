using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogGrokX.Data.Tests;

[TestClass]
public class TransformationPerformerTests
{
    [TestMethod]
    public void Base64DecodeTransformation()
    {
        var performer = new TransformationPerformer(new[] { @"(?<Base64Decode>[A-Za-z0-9+/=]+)" });

        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"a\":1}"));
        var result = performer.Transform($"###{base64}");

        Assert.AreEqual("###{\"a\":1}", result);
    }

    [TestMethod]
    public void DisabledJsonTransformationIsLeftUntouched()
    {
        var performer = new TransformationPerformer(new[] { @"^before (?<FormatJson>\{.*\}) after$" });

        var result = performer.Transform("before {\"a\":1,\"b\":[1,2]} after");

        Assert.AreEqual("before {\"a\":1,\"b\":[1,2]} after", result);
    }
}
