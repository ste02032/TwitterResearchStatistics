using Tweetinvi.Models.V2;

namespace TwitterResearchStatistics.Twitter.Models
{
    public static class StaticStatistics
    {
        public static long TotalTweetsReceived;

        public static long CalculateTotalRank(TweetPublicMetricsV2 publicMetrics)
        {
            if (publicMetrics == null)
                return 0;
            
            return publicMetrics.QuoteCount + publicMetrics.LikeCount + publicMetrics.RetweetCount + publicMetrics.ReplyCount;
        }
    }
}
