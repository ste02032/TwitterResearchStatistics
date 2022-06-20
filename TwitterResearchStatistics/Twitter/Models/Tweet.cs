namespace TwitterResearchStatistics.Twitter.Models
{
    public record Tweet(string Id, DateTimeOffset CreatedAt, string Language, long TotalRank);
}
