using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;

namespace Constructors
{
    namespace BadPractice
    {
        public static class Counter
        {
            public static int Count;

            public static int Increment()
            {
                return ++Count;
            }
        }

        public class Class1
        {
            public int Index { get; private set; }

            public Class1()
            {
                Index = Counter.Increment();
            }
        }

        [TestFixture]
        public class StaticCallTests
        {
            [Test]
            public void FirstTest()
            {
                Class1 c = new Class1();
                
                Assert.AreEqual(1, c.Index);
            }

            [Test]
            public void SecondTest()
            {
                Class1 c1 = new Class1();
                Class1 c2 = new Class1();

                Assert.AreEqual(1, c1.Index);
                Assert.AreEqual(2, c2.Index);
            }
        }

    }

    namespace GoodPractice
    {
        public interface ICounter
        {
            int Increment();
        }

        public class Counter : ICounter
        {
            public int Count;

            public int Increment()
            {
                return ++Count;
            }
        }

        public class Class1
        {
            public int Index { get; private set; }

            public Class1(ICounter counter)
            {
                Index = counter.Increment();
            }
        }

        [TestFixture]
        public class Class1Tests
        {
            private Mock<ICounter> _counterMock;

            [SetUp]
            public void SetUp()
            {
                _counterMock = new Mock<ICounter>(MockBehavior.Strict);
            }

            [TearDown]
            public void TearDown()
            {
                _counterMock.VerifyAll();
            }

            [Test]
            public void FirstTest()
            {
                _counterMock.Setup(x => x.Increment()).Returns(1);

                var c1 = new Class1(_counterMock.Object);

                Assert.AreEqual(1, c1.Index);
            }

            [Test]
            public void SecondTest()
            {
                _counterMock
                    .SetupSequence(x => x.Increment())
                    .Returns(1)
                    .Returns(2);

                int expectedCallCount = 2;

                var c1 = new Class1(_counterMock.Object);
                var c2 = new Class1(_counterMock.Object);

                Assert.AreEqual(1, c1.Index);
                Assert.AreEqual(2, c2.Index);

                _counterMock.Verify(x => x.Increment(), Times.Exactly(expectedCallCount));
            }
        }
    }
}

