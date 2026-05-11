using Dapper;
using Npgsql;

public class AnalyticsAccess
{
    private string _connectionString => AppConfig.PostgresConnectionString;

    public RevenueResult GetRevenueLastMonth()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.QuerySingle<RevenueResult>(
            "SELECT * FROM vw_user_revenue_last_month");
    }

    public RevenueResult GetRevenueLastYear()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.QuerySingle<RevenueResult>(
            "SELECT * FROM vw_user_revenue_last_year");
    }

    public List<PriceChartItem> GetMostExpensiveGames()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.Query<PriceChartItem>(
            "SELECT id, game_name, price FROM vw_user_top_3_most_expensive_games").ToList();
    }

    public List<PriceChartItem> GetCheapestGames()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.Query<PriceChartItem>(
            "SELECT id, game_name, price FROM vw_user_top_3_cheapest_games").ToList();
    }

    public List<SoldChartItem> GetTop3GenresMostSold()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.Query<SoldChartItem>(
            @"SELECT genre_name AS name, sold_copies
              FROM vw_user_top_3_genres_most_sold").ToList();
    }

    public List<SoldChartItem> GetTop10GamesLastMonth()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.Query<SoldChartItem>(
            @"SELECT game_name AS name, sold_copies
              FROM vw_admin_top_10_games_last_month").ToList();
    }

    public List<SoldChartItem> GetTop10Genres()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.Query<SoldChartItem>(
            @"SELECT genre_name AS name, sold_copies
              FROM vw_admin_top_10_genres").ToList();
    }
}