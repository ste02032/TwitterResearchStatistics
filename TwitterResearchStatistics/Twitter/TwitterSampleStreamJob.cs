using Quartz;

namespace TwitterResearchStatistics.Twitter
{
    // prevent this job from running concurrently, only one at a time
    [DisallowConcurrentExecution]
    public class TwitterSampleStreamJob : IJob
    {
        private readonly ITwitterClient _twitterClient;
        public TwitterSampleStreamJob(ITwitterClient twitterClient)
        {
            _twitterClient = twitterClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _twitterClient.GetSampleStreamAsync(context.CancellationToken);
        }
    }
}
