using Serilog;
using System.Collections.Concurrent;
using TwitterResearchStatistics.DAL;
using TwitterResearchStatistics.Twitter.Models;

namespace TwitterResearchStatistics.Twitter
{
    /* Used this StackOverflow for assistance in building out the publisher/consumer pattern
        https://stackoverflow.com/a/6939431
    */

    public class CollectionPersistence
    {
        private readonly ITwitterContext _twitterContext;

        public CollectionPersistence(ITwitterContext twitterContext)
        {
            _twitterContext = twitterContext;
        }

        // Specify a maximum of 10000 items in the collection so that we don't
        // run out of memory if we get data faster than we can commit it.

        BlockingCollection<Tweet> tweetCollection = new BlockingCollection<Tweet>(10000);

        public int TweetCollectionCount
        {
            get
            {
                return tweetCollection.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="takeCollectionTimeout">-1 as default so that it runs indefinitely as more items get added to collection. Modify this to be a positive number to have it timeout and stop waiting</param>
        public void StartQueueConsumerTask(int takeCollectionTimeout = -1, bool forceCloseOnTimeout = false)
        {
            // This is the consumer.  It processes the
            // "tweetCollection" queue until it signals completion.

            while (!tweetCollection.IsCompleted)
            {
                Tweet data;
                HashSet<Tweet> distinctSetOfTweets = new HashSet<Tweet>();
                // Timeout of -1 will wait for an item or IsCompleted == true.

                if (tweetCollection.TryTake(out data, takeCollectionTimeout))
                {
                    distinctSetOfTweets.Add(data);
                    Interlocked.Add(ref StaticStatistics.TotalTweetsReceived, 1);
                    Log.Information($"Tweet Received. Total Count {StaticStatistics.TotalTweetsReceived}");
                    // Continue dequeuing until the queue is empty, where it will
                    // timeout instantly and return false, or until we've dequeued
                    // 100 items.

                    for (int i = 1; i < 100 && tweetCollection.TryTake(out data, 0); ++i)
                    {
                        distinctSetOfTweets.Add(data);
                        Interlocked.Add(ref StaticStatistics.TotalTweetsReceived, 1);
                        Log.Information($"Tweet Received. Total Count {StaticStatistics.TotalTweetsReceived}");
                    }

                    // now that all queued items have been collected or at least 100 of them have been, commit to database.
                    // More can be continue to be added to the queue by other threads while this commit is processing.
                    _twitterContext.SaveTwitterSamplesAsync(distinctSetOfTweets.ToArray());
                }
                else
                {
                    // timed out
                    // determine if collection should be closed or continue to attempt to try and pull from it
                    if (forceCloseOnTimeout)
                        tweetCollection.CompleteAdding();
                }
            }
        }

        // assisted with code from https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-and-take-items
        public bool AddToAsyncCollection(Tweet tweet, CancellationToken ct)
        {
            bool success = false;

            try
            {
                success = tweetCollection.TryAdd(tweet, 200, ct);
            }
            catch (OperationCanceledException ex)
            {
                Log.Error(ex, "Blocking Collection reached capacity and unable to add Tweet until collection frees up.");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Error(ex, "Blocking collection has been disposed");
                throw;
            }

            return success;
        }
    }
}
