
namespace TwitterResearchStatistics.Twitter
{
    public interface ITwitterClient
    {
        public Task GetSampleStreamAsync(CancellationToken cancellationToken);
    }
}
