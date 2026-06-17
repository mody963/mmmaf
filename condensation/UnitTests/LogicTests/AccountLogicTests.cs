namespace UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

[TestClass]
public class AccountsLogicTests
{

    [TestMethod]
    public void CheckLogin_HappyPath_Admin_ReturnsAccount()
    {
        // Arrange
        var fakeAccess = new FakeAccountsAccess();
        // fake Admin account
        fakeAccess.AccountToReturn = new AccountModel { Id = 1, Email = "admin@test.com", Password = "hashedpassword", Role = 1, IsActive = true };
        var logic = new AccountsLogic(fakeAccess);

        // Act
        var result = logic.CheckLogin("admin@test.com", "hashedpassword");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Role); // Verify it's an Admin
        Assert.AreEqual("admin@test.com", result.Email);
    }

    [TestMethod]
    public void CheckLogin_HappyPath_Customer_ReturnsAccount()
    {
        // Arrange
        var fakeAccess = new FakeAccountsAccess();
        // fake Customer account
        fakeAccess.AccountToReturn = new AccountModel { Id = 2, Email = "customer@test.com", Password = "hashedpassword", Role = 3, IsActive = true };
        var logic = new AccountsLogic(fakeAccess);

        // Act
        var result = logic.CheckLogin("customer@test.com", "hashedpassword");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Role); // Verify it's a Customer
    }

    [TestMethod]
    public void CheckLogin_VerkeerdWachtwoord_ReturnsNull()
    {
        // Arrange
        var fakeAccess = new FakeAccountsAccess();
        fakeAccess.AccountToReturn = new AccountModel { Email = "user@test.com", Password = "correctpassword", IsActive = true };
        var logic = new AccountsLogic(fakeAccess);

        // Act Try to login with the wrong password
        var result = logic.CheckLogin("user@test.com", "WRONGpassword");

        // Assert
        Assert.IsNull(result, "Login should fail and return null when the password doesn't match.");
    }

    [TestMethod]
    public void CheckLogin_VerkeerdEmailadres_ReturnsNull()
    {
        // Arrange
        var fakeAccess = new FakeAccountsAccess();
        fakeAccess.AccountToReturn = null; // Simulate email not found in DB
        var logic = new AccountsLogic(fakeAccess);

        // Act
        var result = logic.CheckLogin("unknown@test.com", "password123");

        // Assert
        Assert.IsNull(result, "Login should fail and return null when the email is not found.");
    }

    [TestMethod]
    public void CreateAccount_ThrowsException_IfEmailAlreadyExists()
    {
        // Arrange
        var fakeAccess = new FakeAccountsAccess();
        // email exists
        fakeAccess.AccountToReturn = new AccountModel { Email = "existing@test.com" }; 
        var logic = new AccountsLogic(fakeAccess);

        var newAccount = new AccountModel { Email = "existing@test.com", Password = "pw", FirstName = "Test", LastName = "User" };

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => logic.CreateAccount(newAccount, 3), "Should throw exception for duplicate email.");
    }


    // fake db for tests
    private class FakeAccountsAccess : IAccountsAccess
    {
        public AccountModel? AccountToReturn { get; set; }
        public int CreatedAccountId { get; set; } = 99;

        public AccountModel? GetByEmail(string email)
        {
            // Only return the account if the email matches, otherwise null
            if (AccountToReturn != null && AccountToReturn.Email == email)
                return AccountToReturn;
            return null;
        }

        public int Create(AccountModel account) => CreatedAccountId;
        public void Update(AccountModel account) { }
        public void Delete(int id) { }
        public List<AccountModel> GetAll() => new();
    }
}