using System;

using Aurora.Settings.Bindables;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuroraTests
{
    [TestClass]
    public class BindableTest
    {
        Bindable<string> bindable1;
        Bindable<string> bindable2;

        [TestInitialize]
        public void TestInit()
        {
            bindable1 = new Bindable<string>();
            bindable2 = bindable1.GetBoundCopy();
        }

        [TestMethod]
        public void BindableSame()
        {
            Assert.AreEqual(bindable1.Value, bindable2.Value);

            bindable1.Value = "test";

            Assert.AreEqual(bindable1.Value, bindable2.Value);

            bindable2.Value = "test2";

            Assert.AreEqual(bindable1.Value, bindable2.Value);
        }

        [TestMethod]
        public void BindableNotSame()
        {
            bindable1.UnbindBindings();
            bindable1.Value = "test3";
            Assert.AreNotEqual(bindable1.Value, bindable2.Value);
        }
    }
}
