using Microsoft.Data.Sqlite;
using Serilog;
using TwitterResearchStatistics.Twitter.Models;

namespace TwitterResearchStatistics.DAL
{
    public class TwitterContext : ITwitterContext
    {
        private readonly SqliteConnectionStringBuilder _sqliteConnectionStringBuilder;
        private HashSet<TweetStatistic> _statisticMemoryStore;

        public TwitterContext(SqliteConnectionStringBuilder sqliteConnectionStringBuilder)
        {
            _sqliteConnectionStringBuilder = sqliteConnectionStringBuilder;
            _statisticMemoryStore = new HashSet<TweetStatistic>();
        }

        public async Task SaveTwitterSamplesAsync(IEnumerable<Tweet> tweets)
        {
            // add to memory store for now until database implemented
            foreach (var tweet in tweets)
            {
                _statisticMemoryStore.Add(new TweetStatistic(tweet.Id, tweet.TotalRank));
            }

            //TODO: Perform batch insert of items into the database
            Log.Information($"Batch insert of {tweets.Count()} tweets to database here...");
            //using var connection = new SqliteConnection(_sqliteConnectionStringBuilder.ConnectionString);
            //await connection.OpenAsync();
            //await connection.ExecuteAsync("");
        }

        public IEnumerable<TweetStatistic> GetTop10Tweets()
        {
            return _statisticMemoryStore.OrderByDescending(x => x.Rank).Take(10);
        }
    }
}
