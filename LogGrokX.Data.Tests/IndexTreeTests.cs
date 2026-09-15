using System.Linq;
using LogGrokX.Data.IndexTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogGrokX.Data.Tests
{
    [TestClass]
    public class IndexTreeTests
    {

        [TestMethod]
        public void Smoke()
        {
            const int maxCount = 4000;
            var indexTree = new IndexTree<int, TestIndexTreeLeaf>(16, 
                i => new TestIndexTreeLeaf(i, 0));
            foreach (var value in Enumerable.Range(0, maxCount))
            {
                indexTree.Add(value);
            }

            for (var i = 0; i <= maxCount; i++)
            {
                var enumerable = indexTree.GetEnumerableFromIndex(i).ToList();
                Assert.IsTrue(
                    enumerable.SequenceEqual(Enumerable.Range(i, maxCount - i)),
                    $"Sequence not equal for {i}.");
            }

            for (var i = 0; i < maxCount; i++)
            {
                Assert.AreEqual(indexTree[i], i);
            }
        }
    }
}