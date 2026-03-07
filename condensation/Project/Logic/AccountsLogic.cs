using System;
using System.Collections.Generic;


public class AccountsLogic
{
    private readonly AccountsAccess _accounts = new AccountsAccess();
    
    public AccountModel? CheckLogin(string email, string password)
    {
        var account = _accounts.GetByEmail(email);
        if (account == null || !account.IsActive) return null;
        
        // Note: Storing plain-text passwords isn't secure, but we'll stick to your current logic for now!
        return account.Password == password ? account : null;
    }

    public AccountModel? CheckAdminLogin(string email, string password)
    {
        var account = CheckLogin(email, password);
        if (account == null) return null;
        
        return account.Role == AccountRoles.Admin ? account : null;
    }

    public int CreateAccount(AccountModel account)
    {
        var existing = _accounts.GetByEmail(account.Email);
        if (existing != null) throw new InvalidOperationException("Email already in use");
        return _accounts.Create(account);
    }

    public bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
        return password.IndexOfAny("!@#$%^&*()".ToCharArray()) >= 0;
    }

    public List<AccountModel> GetAllAccounts()
    {
        return _accounts.GetAll();
    }

    public void UpdateAccount(AccountModel account)
    {
        _accounts.Update(account);
    }

    public void DeleteAccount(int id)
    {
        _accounts.Delete(id);
    }
}