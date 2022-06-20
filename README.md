# TwitterResearchStatistics

Sample project that pulls from the Twitter v2 API Sample feed and displays total tweets received as well as top 10 most popular tweets.

The project is built with .NET 6 Razor Pages.
.NET Quartz is used as a job scheduler to manage a job for starting the Twitter sample stream, and one for processing the tweets to in memory storage.

BlockingCollection is utilized in a publisher/consumer to handle the two different threads and keeping a max count to prevent memory from getting too large. 
The index page will display total tweet count received as well as top 10 tweets received based on sum of public metrics per tweet. The page will refresh every 5 seconds automatically.

Commented code is in place to utilize a local SQLite database if desired to persist the tweet information to.

Ideally, if wanting to allow for larger scaled processing of tweets and statistics, Azure Service Bus would be utilized with duplicate prevention and the publisher would write to the service bus. Could have multiple threads processing the consumer which would pull batches from the service bus and persist them to a database and calculate statistics.

Also, an API would be utilized as well so public can pull information from persisted storage.
