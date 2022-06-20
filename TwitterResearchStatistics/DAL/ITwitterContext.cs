using TwitterResearchStatistics.Twitter.Models;

namespace TwitterResearchStatistics.DAL
{
    public interface ITwitterContext
    {
        public Task SaveTwitterSamplesAsync(IEnumerable<Tweet> tweet);
        public IEnumerable<TweetStatistic> GetTop10Tweets();
    }
}
