using Xunit;

public class AnalyticsAdminIntegrationTests
{
    private const string PostgresConn = "Host=localhost;Port=5432;Database=Condensation;Username=menno;Password=test12345";

    private readonly AnalyticsLogic _logic;

    public AnalyticsAdminIntegrationTests()
    {
        AppConfig.PostgresConnectionString = PostgresConn;
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        _logic = new AnalyticsLogic();
    }

    [Fact]
    public void Top10GamesLastMonth_AreSortedDescending_AndMax10()
    {
        var items = _logic.GetTop10GamesLastMonth();
        Assert.True(items.Count <= 10, "Top 10 should not contain more than 10 items.");

        foreach (var it in items)
            Assert.True(it.SoldCopies >= 0, "Sold copies cannot be negative.");

        for (int i = 1; i < items.Count; i++)
            Assert.True(items[i - 1].SoldCopies >= items[i].SoldCopies, "Not sorted descending by sold copies.");
    }

    [Fact]
    public void Top10Genres_AreSortedDescending_AndMax10()
    {
        var items = _logic.GetTop10Genres();
        Assert.True(items.Count <= 10, "Top 10 should not contain more than 10 items.");

        foreach (var it in items)
            Assert.True(it.SoldCopies >= 0, "Sold copies cannot be negative.");

        for (int i = 1; i < items.Count; i++)
            Assert.True(items[i - 1].SoldCopies >= items[i].SoldCopies, "Not sorted descending by sold copies.");
    }
}