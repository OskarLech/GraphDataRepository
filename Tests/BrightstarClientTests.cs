﻿using log4net;
using log4net.Config;
using NUnit.Framework;
using QualityGrapher.Utilities.StructureMap;
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
            XmlConfigurator.Configure(); // Initialize log4net configuration
            ObjectFactory.Container.Configure(c =>
            {
                c.For<ILog>().Use(LogManager.GetLogger(typeof(BrightstarClientTests)));
            });
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
