using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogGrokCore.Tests
{
    [TestClass]
    public class TextModelTests
    {
        private const string Json = "{\"a\":1,\"b\":[1,2]}";

        [TestMethod]
        public void ExpandedJsonIsIndented()
        {
            var model = new TextModel(1, Json);

            var displayed = model.GetDisplayedText(new HashSet<int>());

            StringAssert.Contains(displayed, "\"a\": 1");
            StringAssert.Contains(displayed, "\"b\": [");
        }

        [TestMethod]
        public void CollapsedOuterJsonIsInlined()
        {
            var model = new TextModel(1, Json);
            Assert.IsNotNull(model.CollapsibleRanges);

            var root = model.CollapsibleRanges!.OrderByDescending(r => r.length).First();

            var displayed = model.GetDisplayedText(new HashSet<int> { root.start });

            StringAssert.Contains(displayed, "\"a\": 1");
            StringAssert.Contains(displayed, "\"b\": [1,2]");
            Assert.IsFalse(displayed.Contains('\n'), $"Expected inline json but got: {displayed}");
        }

        [TestMethod]
        public void CollapsedInnerJsonIsInlined()
        {
            var model = new TextModel(1, Json);
            Assert.IsNotNull(model.CollapsibleRanges);

            var inner = model.CollapsibleRanges!.OrderBy(r => r.length).First();

            var displayed = model.GetDisplayedText(new HashSet<int> { inner.start });

            StringAssert.Contains(displayed, "\"b\": [1,2]");
            StringAssert.Contains(displayed, "\"a\": 1");
        }

        [TestMethod]
        public void NullFoldingStateKeepsExpandedJson()
        {
            var model = new TextModel(1, Json);

            var displayed = model.GetDisplayedText(null);

            StringAssert.Contains(displayed, "\"a\": 1");
        }
    }
}
