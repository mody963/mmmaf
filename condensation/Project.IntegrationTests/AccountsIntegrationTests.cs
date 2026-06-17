namespace UnitTests;
using Xunit;
using System;
using Dapper;

public class AccountsIntegrationTests
{   
    [Fact]
    public void CreateAccount_Integration_HappyPath_And_DuplicateThrows()
    {
        // Database Connection
        AppConfig.PostgresConnectionString = "Host=localhost;Port=5433;Database=Condensation;Username=mouhamad;Password=dei2Kaish4dooquiepei";
        
        // Tell Dapper how to read the "is_active" database column is_active to IsActive
        SqlMapper.AddTypeMap(typeof(string), System.Data.DbType.AnsiString); // Optional safety
        DefaultTypeMap.MatchNamesWithUnderscores = true; // fix otherwise it doesnt read
    
        // Arrange
        var logic = new AccountsLogic();
        string uniqueEmail = $"integrationtest_{Guid.NewGuid()}@test.com";
        
        var newAccount = new AccountModel 
        { 
            Email = uniqueEmail, 
            Password = "hashedpassword", 
            FirstName = "Test", 
            LastName = "User",
            Role = 3, // Customer
            IsActive = true
        };

        int createdAccountId = 0;

        try
        {
            // happy path
            // Act
            createdAccountId = logic.CreateAccount(newAccount, 3);

            // Assert
            Assert.True(createdAccountId > 0, "The database should generate a valid positive ID.");
            
            var savedAccount = logic.CheckLogin(uniqueEmail, "hashedpassword");
            Assert.NotNull(savedAccount);

            // duplicate account
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
            {
                logic.CreateAccount(newAccount, 3);
            });
            Assert.Equal("Email already in use", exception.Message); 
        }
        finally
        {
            // delete data
            if (createdAccountId > 0)
            {
                logic.DeleteAccount(createdAccountId);
            }
        }
    }
}