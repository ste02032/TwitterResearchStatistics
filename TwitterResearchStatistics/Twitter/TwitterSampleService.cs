using Serilog;
using TwitterResearchStatistics.DAL;

namespace TwitterResearchStatistics.Twitter
{
    public class TwitterSampleService
    {
        private readonly ITwitterClient _twitterClient;

        public TwitterSampleService(ITwitterClient twitterClient)
        {
            _twitterClient = twitterClient;
        }

        public async Task ProcessSamples(CancellationToken cancellationToken)
        {
            // start the stream to the twitter client and add tweets to the queue to be persisted to database by another scheduled job
            await _twitterClient.GetSampleStreamAsync(cancellationToken);
        }
    }
}
