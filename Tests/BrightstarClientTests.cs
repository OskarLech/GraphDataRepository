using NUnit.Framework;
using Rhino.Mocks;

namespace Tests
{
    [TestFixture]
    internal class BrightstarClientTests //TODO : ITriplestoreClient
    {
        private readonly MockRepository _mock = new MockRepository();

        [OneTimeSetUp]
        public void TestFixtureSetup()
        {
        }

        [Test]
        [MaxTime(1000)]
        public void PlaceholderTest()
        {
            Assert.IsTrue(true);
        }

        [TearDown]
        public void TearDown()
        {
            _mock.VerifyAll();
        }
    }
}
