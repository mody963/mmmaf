using System.Collections.Generic;

public interface IAccountsAccess
{
    AccountModel? GetByEmail(string email);
    int Create(AccountModel account);
    void Update(AccountModel account);
    void Delete(int id);
    List<AccountModel> GetAll();
}