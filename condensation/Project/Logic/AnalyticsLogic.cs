public class AnalyticsLogic
{
    private readonly AnalyticsAccess _analyticsAccess = new AnalyticsAccess();

    public RevenueResult GetRevenueLastMonth() => _analyticsAccess.GetRevenueLastMonth();
    public RevenueResult GetRevenueLastYear() => _analyticsAccess.GetRevenueLastYear();

    public List<PriceChartItem> GetMostExpensiveGames() => _analyticsAccess.GetMostExpensiveGames();
    public List<PriceChartItem> GetCheapestGames() => _analyticsAccess.GetCheapestGames();

    public List<SoldChartItem> GetTop3GenresMostSold() => _analyticsAccess.GetTop3GenresMostSold();
    public List<SoldChartItem> GetTop10GamesLastMonth() => _analyticsAccess.GetTop10GamesLastMonth();
    public List<SoldChartItem> GetTop10Genres() => _analyticsAccess.GetTop10Genres();
}