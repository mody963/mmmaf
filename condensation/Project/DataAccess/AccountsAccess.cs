using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Linq;

public class AccountsAccess
{
    //private readonly string _connectionString = AppConfig.ConnectionString;
    private string _connectionString => AppConfig.PostgresConnectionString;
    private const string Table = "account";

    public AccountModel? GetByEmail(string email)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = $"SELECT * FROM {Table} WHERE email = @Email";
        return connection.QueryFirstOrDefault<AccountModel>(sql, new { Email = email });
    }

    public int Create(AccountModel account)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = $@"
            INSERT INTO {Table} (email, first_name, last_name, password, role, is_active)
            VALUES (@Email, @FirstName, @LastName, @Password, @Role, @IsActive)
            RETURNING id;";
        return connection.ExecuteScalar<int>(sql, account);
    }

    public void Update(AccountModel account)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = $@"
            UPDATE {Table} SET
                email = @Email,
                first_name = @FirstName,
                last_name = @LastName,
                password = @Password,
                role = @Role,
                is_active = @IsActive
            WHERE id = @Id";
        connection.Execute(sql, account);
    }

    public void Delete(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = $"DELETE FROM {Table} WHERE id = @Id";
        connection.Execute(sql, new { Id = id });
    }

    public List<AccountModel> GetAll()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = $"SELECT * FROM {Table}";
        return connection.Query<AccountModel>(sql).ToList();
    }
}