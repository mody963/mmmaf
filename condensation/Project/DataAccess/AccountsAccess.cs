using Microsoft.Data.Sqlite;

public class AccountsAccess
{
    private string connectionString = $"Data Source={DbPath.Get()}";

    public AccountModel? GetByEmail(string email)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        string sql = $"SELECT * FROM accounts WHERE email = '{email}'";

        var command = new SqliteCommand(sql, connection);
        var reader = command.ExecuteReader();

        if (reader.Read())
        {
            return new AccountModel
            {
                Id = Convert.ToInt32(reader["id"]),
                Email = reader["email"].ToString() ?? "",
                FirstName = reader["firstName"].ToString() ?? "",
                LastName = reader["lastName"].ToString() ?? "",
                Password = reader["password"].ToString() ?? "",
                Role = Convert.ToInt32(reader["role"]),
                IsActive = Convert.ToBoolean(reader["isActive"])
            };
        }

        return null;
    }

    public int Create(AccountModel account)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        string sql =
        $"INSERT INTO accounts (email, firstName, lastName, password, role, isActive) " +
        $"VALUES ('{account.Email}', '{account.FirstName}', '{account.LastName}', '{account.Password}', {account.Role}, {account.IsActive}); " +
        $"SELECT last_insert_rowid();";

        var command = new SqliteCommand(sql, connection);

        int newId = Convert.ToInt32(command.ExecuteScalar());

        return newId;
    }

    public void Update(AccountModel account)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        string sql =
        $"UPDATE accounts SET " +
        $"email = '{account.Email}', " +
        $"firstName = '{account.FirstName}', " +
        $"lastName = '{account.LastName}', " +
        $"password = '{account.Password}', " +
        $"role = {account.Role}, " +
        $"isActive = {account.IsActive} " +
        $"WHERE id = {account.Id}";

        var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        string sql = $"DELETE FROM accounts WHERE id = {id}";

        var command = new SqliteCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public List<AccountModel> GetAll()
    {
        List<AccountModel> accounts = new List<AccountModel>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        string sql = "SELECT * FROM accounts";

        var command = new SqliteCommand(sql, connection);
        var reader = command.ExecuteReader();

        while (reader.Read())
        {
            AccountModel account = new AccountModel
            {
                Id = Convert.ToInt32(reader["id"]),
                Email = reader["email"].ToString() ?? "",
                FirstName = reader["firstName"].ToString() ?? "",
                LastName = reader["lastName"].ToString() ?? "",
                Password = reader["password"].ToString() ?? "",
                Role = Convert.ToInt32(reader["role"]),
                IsActive = Convert.ToBoolean(reader["isActive"])
            };

            accounts.Add(account);
        }

        return accounts;
    }
}