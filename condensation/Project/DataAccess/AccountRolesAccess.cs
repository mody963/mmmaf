using Npgsql;
using Dapper;

public class AccountRolesAccess
{
    private string _connectionString => AppConfig.PostgresConnectionString;

    public void Assign(int accountId, int roleId)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = @"
            INSERT INTO account_roles (account_id, role_id)
            VALUES (@AccountId, @RoleId)
            ON CONFLICT (account_id, role_id) DO NOTHING";
        connection.Execute(sql, new { AccountId = accountId, RoleId = roleId });
    }
}