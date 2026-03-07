using Npgsql;
using System;
using System.Collections.Generic;

namespace CondensationApp; // Assuming this is your namespace based on your Database class

public class AccountsAccess
{
    private readonly string _connectionString;

    // Pass the connection string in when you create this class
    public AccountsAccess(string connectionString)
    {
        _connectionString = connectionString;
    }

    public AccountModel? GetByEmail(string email)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // Use @email as a parameter placeholder to prevent SQL injection
        string sql = "SELECT * FROM accounts WHERE email = @email";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email); // Safely inject the value here

        using var reader = command.ExecuteReader();

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
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        // Postgres uses RETURNING id at the end of the INSERT statement
        string sql = @"
            INSERT INTO accounts (email, firstName, lastName, password, role, isActive) 
            VALUES (@email, @firstName, @lastName, @password, @role, @isActive) 
            RETURNING id;";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", account.Email);
        command.Parameters.AddWithValue("@firstName", account.FirstName);
        command.Parameters.AddWithValue("@lastName", account.LastName);
        command.Parameters.AddWithValue("@password", account.Password);
        command.Parameters.AddWithValue("@role", account.Role);
        command.Parameters.AddWithValue("@isActive", account.IsActive);

        // ExecuteScalar returns the first column of the first row (which is our returning ID)
        int newId = Convert.ToInt32(command.ExecuteScalar());

        return newId;
    }

    public void Update(AccountModel account)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        string sql = @"
            UPDATE accounts SET 
                email = @email, 
                firstName = @firstName, 
                lastName = @lastName, 
                password = @password, 
                role = @role, 
                isActive = @isActive 
            WHERE id = @id";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", account.Email);
        command.Parameters.AddWithValue("@firstName", account.FirstName);
        command.Parameters.AddWithValue("@lastName", account.LastName);
        command.Parameters.AddWithValue("@password", account.Password);
        command.Parameters.AddWithValue("@role", account.Role);
        command.Parameters.AddWithValue("@isActive", account.IsActive);
        command.Parameters.AddWithValue("@id", account.Id);

        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        string sql = "DELETE FROM accounts WHERE id = @id";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        
        command.ExecuteNonQuery();
    }

    public List<AccountModel> GetAll()
    {
        List<AccountModel> accounts = new List<AccountModel>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        string sql = "SELECT * FROM accounts";

        using var command = new NpgsqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

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