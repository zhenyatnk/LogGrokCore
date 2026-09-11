using System;
using System.Threading;
using LogGrokCore.Controls.TextRender;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LogGrokCore.Tests
{
    [TestClass]
    public class FoldingStateSharingTests
    {
        [TestMethod]
        public void CollapseInOneTextViewPropagatesToSharedStateClients()
        {
            RunOnSta(() =>
            {
                const string json = "{\"a\":{\"b\":1,\"c\":2}}";
                var state = new TextViewSharedFoldingState();
                var first = new TextView();
                var second = new TextView();
                TextView.SetSharedFoldingState(first, state);
                TextView.SetSharedFoldingState(second, state);
                first.TextModel = new TextModel(42, json);
                second.TextModel = new TextModel(42, json);

                Assert.IsTrue(first.FoldingManager!.CollapseRecursivelyCommand!.CanExecute(null));
                Assert.IsTrue(second.FoldingManager!.CollapseRecursivelyCommand!.CanExecute(null));

                first.FoldingManager.CollapseRecursivelyCommand!.Execute(null);

                Assert.IsFalse(first.FoldingManager.CollapseRecursivelyCommand.CanExecute(null));
                Assert.IsFalse(second.FoldingManager!.CollapseRecursivelyCommand!.CanExecute(null));
            });
        }

        [TestMethod]
        public void ToggleInOneTextViewIsNotPropagatedToDifferentUniqueId()
        {
            RunOnSta(() =>
            {
                const string json = "{\"a\":{\"b\":1,\"c\":2}}";
                var state = new TextViewSharedFoldingState();
                var first = new TextView();
                var other = new TextView();
                TextView.SetSharedFoldingState(first, state);
                TextView.SetSharedFoldingState(other, state);
                first.TextModel = new TextModel(1, json);
                other.TextModel = new TextModel(2, json);

                first.FoldingManager!.CollapseRecursivelyCommand!.Execute(null);

                Assert.IsFalse(first.FoldingManager.CollapseRecursivelyCommand.CanExecute(null));
                Assert.IsTrue(other.FoldingManager!.CollapseRecursivelyCommand!.CanExecute(null));
            });
        }

        private static void RunOnSta(Action action)
        {
            Exception error = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    error = e;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (error != null)
                throw error;
        }
    }
}
