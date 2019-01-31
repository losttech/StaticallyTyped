namespace LostTech.StaticallyTyped {
    using IronPython.Hosting;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    [TestClass]
    public class PropertyTests {
        [TestMethod]
        public void CanReadPrimitives() {
            var engine = Python.CreateEngine();
            var scope = engine.CreateScope();
            const int value = 42;
            scope.SetVariable(nameof(IPrimitivePropertyContainer.Property), value);
            var container = scope.AssumeType<IPrimitivePropertyContainer>();
            Assert.AreEqual(value, container.Property);
        }

        internal interface IPrimitivePropertyContainer
        {
            int Property { get; }
        }
    }
}
