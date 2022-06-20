using Quartz;

namespace TwitterResearchStatistics.Twitter
{
    [DisallowConcurrentExecution]
    public class TwitterQueueConsumerJob : IJob
    {
        private readonly CollectionPersistence _collectionPersistence;
        public TwitterQueueConsumerJob(CollectionPersistence collectionPersistence)
        {
            _collectionPersistence = collectionPersistence;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _collectionPersistence.StartQueueConsumerTask();
        }
    }
}
