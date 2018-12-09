using System.Xml;
using Moq;
using NUnit.Framework;

namespace Constructors
{
    namespace BadPractice
    {
        public class XMLReader
        {
            public XmlDocument Document { get; private set; }

            public XMLReader()
            {
                // load xml file here
                Document = new XmlDocument();
            }
        }

        public class PathFinder
        {
            public PathFinder()
            {
                XMLReader reader = new XMLReader();

                if (reader.Document.HasChildNodes)
                {
                    // set some state
                }
            }

            public bool Find(string query)
            {
                // find logic here
                return true;
            }
        }

        [TestFixture]
        public class PathFinderTests
        {
            [Test]
            public void QuerySimpleTagTest()
            {
                string query = "mytag";
                PathFinder pathFinder = new PathFinder();
                bool result = pathFinder.Find(query);

                Assert.IsTrue(result);
            }
        }

    }

    namespace GoodPractice
    {
        public interface IDocumentSource
        {
            XmlDocument Document { get; }
            void LoadDocument();
        }

        public class XMLReader : IDocumentSource
        {
            public XmlDocument Document { get; private set; }
            
            public void LoadDocument()
            {
                // load document here
                Document = new XmlDocument();
            }
        }

        public class PathFinder
        {
            private readonly IDocumentSource _documentSource;

            public PathFinder(IDocumentSource documentSource)
            {
                _documentSource = documentSource;
            }

            public void ReadDocument()
            {
                _documentSource.LoadDocument();
            }

            public bool Find(string query)
            {
                // find logic here
                return true;
            }
        }

        [TestFixture]
        public class PathFinderTests
        {
            private Mock<IDocumentSource> _documentSourceMock;

            [SetUp]
            public void SetUp()
            {
                _documentSourceMock = new Mock<IDocumentSource>(MockBehavior.Strict);
            }

            [TearDown]
            public void TearDown()
            {
                _documentSourceMock.VerifyAll();
            }
            
            [Test]
            public void QuerySimpleTagTest()
            {
                string query = "mytag";
                
                _documentSourceMock.Setup(x => x.LoadDocument());

                PathFinder pathFinder = new PathFinder(_documentSourceMock.Object);
                pathFinder.ReadDocument();
                bool result = pathFinder.Find(query);

                Assert.IsTrue(result);
            }
        }
    }
}

