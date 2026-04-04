using System;
using System.Collections.Generic;


public class AccountsLogic
{
    private readonly AccountsAccess _accounts = new AccountsAccess();
    
    public AccountModel? CheckLogin(string email, string password)
    {
        var account = _accounts.GetByEmail(email);
        
        // 1. something is wrong
        if (account == null || account.Password != password) 
            return null;
            
        // 2. inactive account
        if (!account.IsActive)
        {
            throw new UnauthorizedAccessException("Your account is currently inactive or pending Admin approval.");
        }
        return account;
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

    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        int atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex >= email.Length - 1) return false; // @ not at start or end

        int dotIndex = email.LastIndexOf('.');
        if (dotIndex <= atIndex || dotIndex >= email.Length - 1) return false; // . after @ and not at end

        // Ensure there's at least one char after the last .
        if (dotIndex == email.Length - 1) return false;

        return true;
    }
    public bool IsValidName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return false;

        foreach (char c in name)
        {
            if (!char.IsLetter(c) && c != ' ' && c != '-' && c != '\'')
                return false;
        }

        return true;
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
    public List<AccountModel> GetPendingPublisherAccounts()
    {
        // Grabs all accounts, then filters for Publishers (Role 2) that are currently inactive
        return _accounts.GetAll()
            .Where(a => a.Role == AccountRoles.Publisher && !a.IsActive)
            .ToList();
    }
}