using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Linq;

public class CustomerAccess
{
    private string _connectionString => AppConfig.ConnectionString;
    public int CreateCustomer(CustomerModel customer)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = $@"
        INSERT INTO customer (account_id, payment_method, address)
        VALUES (@AccountId, @PaymentMethod, @Address)
        RETURNING id;";
        return connection.ExecuteScalar<int>(sql, customer);
    }
    public CustomerModel? GetByAccountId(int accountId)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        string sql = "SELECT * FROM customer WHERE account_id = @AccountId";

        return connection.QueryFirstOrDefault<CustomerModel>(sql, new { AccountId = accountId });
    }
    public void Update(CustomerModel customer)
    {
        using var connection = new NpgsqlConnection(_connectionString);

        string sql = @"
        UPDATE customer
        SET payment_method = @PaymentMethod,
            address = @Address
        WHERE account_id = @AccountId";

        connection.Execute(sql, customer);
    }
}
