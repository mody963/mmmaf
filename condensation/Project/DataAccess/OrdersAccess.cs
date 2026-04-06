using Dapper;
using Npgsql;

public class OrdersAccess
{
    private string ConnectionString => AppConfig.ConnectionString;

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
}