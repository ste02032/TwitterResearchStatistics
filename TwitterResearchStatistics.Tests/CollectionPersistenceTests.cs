using TwitterResearchStatistics.Twitter;
using Moq;
using TwitterResearchStatistics.DAL;

namespace TwitterResearchStatistics.Tests
{
    [TestClass]
    public class CollectionPersistenceTests
    {
        private ITwitterContext _twitterContext;

        [TestInitialize]
        public void Init()
        {
            _twitterContext = new Mock<ITwitterContext>().Object;
        }

        [TestMethod]
        public void AddToCollection_ExpectedFailCount_Success()
        {
            var collectionPersistence = new CollectionPersistence(_twitterContext);
            int maxIterations = 10005;

            int failedCount = 0;
            for(int i = 0; i < maxIterations; i++)
            {
                var success = collectionPersistence.AddToAsyncCollection(new Twitter.Models.Tweet(i.ToString(), DateTime.UtcNow, "english", i), CancellationToken.None);
                if (!success)
                    failedCount++;
            }

            Assert.AreEqual(5, failedCount);
        }

        [TestMethod]
        public void AddToCollection_ExpectedSuccessCount_Success()
        {
            var collectionPersistence = new CollectionPersistence(_twitterContext);
            int maxIterations = 200;

            int failedCount = 0;
            for (int i = 0; i < maxIterations; i++)
            {
                var success = collectionPersistence.AddToAsyncCollection(new Twitter.Models.Tweet(i.ToString(), DateTime.UtcNow, "english", i), CancellationToken.None);
                if (!success)
                    failedCount++;
            }

            Assert.AreEqual(0, failedCount);
        }

        [TestMethod]
        public void ProcessCollection_Success()
        {
            var collectionPersistence = new CollectionPersistence(_twitterContext);
            int maxIterations = 200;

            int failedCount = 0;
            int processedCount = 0;

            for (int i = 0; i < maxIterations; i++)
            {
                var success = collectionPersistence.AddToAsyncCollection(new Twitter.Models.Tweet(i.ToString(), DateTime.UtcNow, "english", i), CancellationToken.None);
                if (!success)
                    failedCount++;
                else processedCount++;
            }

            // now process queue
            var timeoutMilliseconds = 200;
            bool forceCloseOnTimeout = true;
            collectionPersistence.StartQueueConsumerTask(timeoutMilliseconds, forceCloseOnTimeout);

            Assert.AreEqual(0, failedCount);
            Assert.AreEqual(200, processedCount);
            // confirm that collection count is empty meaning it processed
            Assert.AreEqual(0, collectionPersistence.TweetCollectionCount);
        }
    }
}