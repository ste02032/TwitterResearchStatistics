using Serilog;
using System.Text.Json;
using Tweetinvi.Parameters.TrendsClient;
using Tweetinvi.Parameters.V2;
using TwitterResearchStatistics.Twitter.Models;
using tweet = Tweetinvi;

namespace TwitterResearchStatistics.Twitter
{
    public class TwitterClient : ITwitterClient
    {
        private readonly tweet.TwitterClient _twitterApiClient;
        private readonly CollectionPersistence _collectionPersistence;
        public TwitterClient(string apiKey, string apiSecret, string apiBearerToken, CollectionPersistence collectionPersistence)
        {
            _collectionPersistence = collectionPersistence;
            _twitterApiClient = new tweet.TwitterClient(apiKey, apiSecret, apiBearerToken);
            // by setting the rate limit mode to await it will hold the thread until rate limit time period has elapsed and can start again
            // since we only have one job in a thread at a time this is okay for now, if ever doing multiple threaded same jobs then would want to handle rate limits manually
            _twitterApiClient.Config.RateLimitTrackerMode = tweet.RateLimitTrackerMode.TrackAndAwait;
        }

        public async Task GetSampleStreamAsync(CancellationToken cancellationToken)
        {
            // get rate limits if manually handling them
            bool manualRateLimitCheck = false;
            int totalRemainingTweetsAllowed = int.MaxValue;
            if (_twitterApiClient.Config.RateLimitTrackerMode != tweet.RateLimitTrackerMode.TrackAndAwait)
            {
                manualRateLimitCheck = true;
                var rateLimits = await _twitterApiClient.RateLimits.GetRateLimitsAsync();
                var searchTweetsLimits = rateLimits.SearchTweetsLimit;

                // get remaining rate limits
                totalRemainingTweetsAllowed = searchTweetsLimits.Remaining;
                // check for other rate limits here and determine how to handle them
            }

            var sampleStreamV2 = _twitterApiClient.StreamsV2.CreateSampleStream();
            sampleStreamV2.TweetReceived += (sender, args) =>
            {
                // if thread has been cancelled or no more tweets allowed per rate limiting then stop stream
                if (cancellationToken.IsCancellationRequested || (manualRateLimitCheck && totalRemainingTweetsAllowed <= 0))
                    sampleStreamV2.StopStream();
                if (args.Tweet != null)
                {
                    // add the information we want to keep into new Tweet class object that will be persisted to a blocking collection for thread safe usage
                    _collectionPersistence.AddToAsyncCollection(new Tweet(args.Tweet.Id, args.Tweet.CreatedAt, args.Tweet.Lang, StaticStatistics.CalculateTotalRank(args.Tweet.PublicMetrics)), cancellationToken);
                    if (manualRateLimitCheck)
                        --totalRemainingTweetsAllowed;
                }
                else if (!string.IsNullOrEmpty(args.Json))
                {
                    // we should only get to this point if client config is not handling rate limiting automatically
                    // perform manual rate limiting fallbacks
                    // most likely rate limits have occurred or some other error, try to determine what happened by looking at returned JSON
                    var unknownObject = JsonDocument.Parse(args.Json);
                    var title = unknownObject.RootElement.GetProperty("title").GetString()?.ToLower();

                    if (!string.IsNullOrEmpty(title))
                    {
                        var messageDetail = unknownObject.RootElement.GetProperty("detail").GetString()?.ToLower();
                        var messageType = unknownObject.RootElement.GetProperty("type").GetString()?.ToLower();

                        switch (title)
                        {
                            case "connectionexception":
                                
                                var connectionIssue = unknownObject.RootElement.GetProperty("connection_issue").GetString()?.ToLower();
                                // have reached the collection limit
                                //TODO: Perform logic that would prevent the job from running for a specified period of time until connection limit has been removed
                                Log.Error($"Rate limiting connection exception occurred ({connectionIssue}). {messageDetail} {messageType}");
                                break;
                            case "operational-disconnect":
                                var disconnectType = unknownObject.RootElement.GetProperty("disconnect_type").GetString()?.ToLower();
                                // have reached the collection limit
                                //TODO: Perform logic that would prevent the job from running for a specified period of time until connection limit has been removed
                                Log.Error($"Rate limiting operational disconnect occurred ({disconnectType}). {messageDetail} {messageType}");
                                break;
                                /*
                                 * TODO: perform other error handling here
                                 */
                                default:
                                Log.Warning($"Unhandled JSON response when no tweet returned. {title}. {messageDetail} {messageType}");
                                // TODO: handle this scenario
                                break;
                        }
                    }
                }
            };
            await sampleStreamV2.StartAsync(new StartSampleStreamV2Parameters() {  });
        }
    }
}
