using Microsoft.AspNetCore.Mvc.RazorPages;
using TwitterResearchStatistics.DAL;
using TwitterResearchStatistics.Twitter.Models;

namespace TwitterResearchStatistics.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ITwitterContext _twitterContext;

        public long TotalTweetCount = StaticStatistics.TotalTweetsReceived;

        public IEnumerable<TweetStatistic> TopTweets { get; private set; }

        public IndexModel(ILogger<IndexModel> logger, ITwitterContext twitterContext)
        {
            _logger = logger;
            _twitterContext = twitterContext;
        }

        public void OnGet()
        {
            TopTweets = _twitterContext.GetTop10Tweets();
        }
    }
}