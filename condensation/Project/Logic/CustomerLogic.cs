public class CustomersLogic
{
    private readonly CustomerAccess _customers = new CustomerAccess();

    public CustomerModel? GetByAccountId(int accountId)
    {
        return _customers.GetByAccountId(accountId);
    }

    public void CreateCustomer(CustomerModel customer)
    {
        _customers.CreateCustomer(customer);
    }

    public void UpdateCustomer(CustomerModel customer)
    {
        _customers.Update(customer);
    }

    public bool IsValidPaymentMethod(string payment)
    {
        if (string.IsNullOrWhiteSpace(payment))
            return false;

        payment = payment.ToLower();

        return payment == "iban"
            || payment == "creditcard"
            || payment == "paypal";
    }

    public bool IsValidAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return false;

        return address.Length >= 6;
    }
}