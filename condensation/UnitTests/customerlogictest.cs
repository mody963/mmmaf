namespace UnitTests;

[TestClass]
public class CustomerLogicTests
{
    [TestMethod]
    public void IsValidEmail_ReturnsTrue_ForValidAddresses()
    {
        var logic = new CustomersLogic();

        Assert.IsTrue(logic.IsValidEmail("user@example.com"));
        Assert.IsTrue(logic.IsValidEmail("test.user@domain.co"));
    }

    [TestMethod]
    public void IsValidEmail_ReturnsFalse_ForInvalidAddresses()
    {
        var logic = new CustomersLogic();

        Assert.IsFalse(logic.IsValidEmail("userexample.com"));
        Assert.IsFalse(logic.IsValidEmail("user@domain"));
        Assert.IsFalse(logic.IsValidEmail(""));
        Assert.IsFalse(logic.IsValidEmail(null));
    }

    [TestMethod]
    public void IsValidPaymentMethod_ReturnsTrue_ForSupportedMethods()
    {
        var logic = new CustomersLogic();

        Assert.IsTrue(logic.IsValidPaymentMethod("IBAN"));
        Assert.IsTrue(logic.IsValidPaymentMethod("creditcard"));
        Assert.IsTrue(logic.IsValidPaymentMethod("PayPal"));
    }

    [TestMethod]
    public void IsValidPaymentMethod_ReturnsFalse_ForUnsupportedMethods()
    {
        var logic = new CustomersLogic();

        Assert.IsFalse(logic.IsValidPaymentMethod("bitcoin"));
        Assert.IsFalse(logic.IsValidPaymentMethod(""));
        Assert.IsFalse(logic.IsValidPaymentMethod(null));
    }

    [TestMethod]
    public void IsValidAddress_ReturnsTrue_ForValidAddresses()
    {
        var logic = new CustomersLogic();

        Assert.IsTrue(logic.IsValidAddress("123 Main St"));
        Assert.IsTrue(logic.IsValidAddress("Amsterdam 45"));
    }

    [TestMethod]
    public void IsValidAddress_ReturnsFalse_ForTooShortOrEmptyAddress()
    {
        var logic = new CustomersLogic();

        Assert.IsFalse(logic.IsValidAddress(""));
        Assert.IsFalse(logic.IsValidAddress("123"));
        Assert.IsFalse(logic.IsValidAddress(null));
    }
}
