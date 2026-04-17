using Dapper;
using Npgsql;

public class OrdersAccess
{
    private string ConnectionString => AppConfig.PostgresConnectionString;

    public int CreateOrder(int customerId, double totalPrice, List<CartModel> items)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            const string orderSql = @"
                INSERT INTO orders (customer_id, total_price)
                VALUES (@CustomerId, @TotalPrice)
                RETURNING id;";

            int orderId = connection.ExecuteScalar<int>(orderSql, new
            {
                CustomerId = customerId,
                TotalPrice = totalPrice
            },
                transaction
            );

            const string orderGameSql = @"
                INSERT INTO order_games (game_id, order_id, quantity)
                VALUES (@GameId, @OrderId, @Quantity);";

            foreach (var item in items)
            {
                connection.Execute(
                    orderGameSql,
                    new
                    {
                        GameId = item.id,
                        OrderId = orderId,
                        Quantity = 1
                    },
                    transaction
                );
            }

            transaction.Commit();
            return orderId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
    public List<GameModel> GetOwnedGamesByCustomerId(int customerId)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        const string sql = @"
            SELECT DISTINCT g.*
            FROM orders o
            JOIN order_games og ON og.order_id = o.id
            JOIN game g ON g.id = og.game_id
            WHERE o.customer_id = @CustomerId
            ORDER BY g.title;";

        return connection.Query<GameModel>(sql, new { CustomerId = customerId }).ToList();
    }

    public bool HasPurchasedGame(int customerId, int gameId)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        const string sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM orders o
                JOIN order_games og ON og.order_id = o.id
                WHERE o.customer_id = @CustomerId
                  AND og.game_id = @GameId
            );";

        return connection.ExecuteScalar<bool>(sql, new { CustomerId = customerId, GameId = gameId });
    }
}