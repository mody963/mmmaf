using Xunit;

public class ViewsUserIntegrationTests
{
    private const string PostgresConn = "Host=localhost;Port=5432;Database=Condensation;Username=menno;Password=test12345";

    private readonly AnalyticsLogic _logic;

    public ViewsUserIntegrationTests()
    {
        AppConfig.PostgresConnectionString = PostgresConn;
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        _logic = new AnalyticsLogic();
    }

    [Fact]
    public void RevenueLastMonth_HasValidPeriod_AndNonNegativeTotal()
    {
        var r = _logic.GetRevenueLastMonth();
        Assert.True(r.PeriodEnd >= r.PeriodStart, "Period end is before the period start.");
        Assert.True(r.TotalRevenue >= 0, "Revenue cannot be negative.");
    }

    [Fact]
    public void RevenueLastYear_HasValidPeriod_AndNonNegativeTotal()
    {
        var r = _logic.GetRevenueLastYear();
        Assert.True(r.PeriodEnd >= r.PeriodStart, "Period end is before the period start.");
        Assert.True(r.TotalRevenue >= 0, "Revenue cannot be negative.");
    }

    [Fact]
    public void MostExpensiveGames_AreSortedDescending_AndMax3()
    {
        var items = _logic.GetMostExpensiveGames();
        Assert.True(items.Count <= 3, "Top 3 should not contain more than 3 items.");

        // most expensive first so each one should be >= the next
        for (int i = 1; i < items.Count; i++)
            Assert.True(items[i - 1].Price >= items[i].Price, "Not sorted descending by price.");
    }

    [Fact]
    public void CheapestGames_AreSortedAscending_AndMax3()
    {
        var items = _logic.GetCheapestGames();
        Assert.True(items.Count <= 3, "Top 3 should not contain more than 3 items.");

        // cheapest first this time so the order flips
        for (int i = 1; i < items.Count; i++)
            Assert.True(items[i - 1].Price <= items[i].Price, "Not sorted ascending by price.");
    }

    [Fact]
    public void Top3GenresMostSold_AreSortedDescending_AndMax3()
    {
        var items = _logic.GetTop3GenresMostSold();
        Assert.True(items.Count <= 3, "Top 3 should not contain more than 3 items.");

        foreach (var it in items)
            Assert.True(it.SoldCopies >= 0, "Sold copies cannot be negative.");

        for (int i = 1; i < items.Count; i++)
            Assert.True(items[i - 1].SoldCopies >= items[i].SoldCopies, "Not sorted descending by sold copies.");
    }
}