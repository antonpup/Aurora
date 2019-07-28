using System;

using Aurora.Settings.Bindables;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AuroraTests
{
    [TestClass]
    public class BindableTest
    {
        BindableBindableDictionary bindable1;

        [TestInitialize]
        public void TestInit()
        {
            bindable1 = new BindableBindableDictionary();
        }

        [TestMethod]
        public void TestUpdateEvent()
        {
            bindable1.ItemsAdded += pairs =>
            {
                foreach (var pair in pairs)
                {
                    Assert.IsTrue(pair.Value == bindable1[pair.Key]);
                }
            };
            bindable1.ItemsRemoved += pairs =>
            {
                foreach (var pair in pairs)
                {
                    Assert.IsFalse(bindable1.ContainsKey(pair.Key));
                }
            };
            bindable1.Add("test", new BindableBool());
            ((BindableBool) bindable1["test"]).Value = true;
            bindable1.Remove("test");
        }
    }
}
