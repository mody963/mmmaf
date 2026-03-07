public class AccountsLogic
{
    private readonly AccountsAccess accounts = new AccountsAccess();
    
    public AccountModel? CheckLogin(string email, string password)
    {
        var account = accounts.GetByEmail(email);
        if (account == null || !account.IsActive) return null;
        return account.Password == password? account : null;
    }
    public AccountModel? CheckAdminLogin(string email, string password)
    {
        var account = CheckLogin(email, password);
        if (account == null) return null;
        return account.Role == AccountRoles.Admin ? account : null;
    }

    public int CreateAccount(AccountModel account)
    {
        var existing = accounts.GetByEmail(account.Email);
        if (existing != null) throw new InvalidOperationException("Email already in use");
        return accounts.Create(account);
    }
    public bool IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8) return false;
        return password.IndexOfAny("!@#$%^&*()".ToCharArray()) >= 0;
    }
    public List<AccountModel> GetAllAccounts()
    {
        return accounts.GetAll();
    }

    public void UpdateAccount(AccountModel account)
    {
        accounts.Update(account);
    }

    public void DeleteAccount(int id)
    {
        accounts.Delete(id);
    }
}



