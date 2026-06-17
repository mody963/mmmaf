namespace UnitTests;

[TestClass]
public class CartLogicTests
{
    [TestMethod]
    public void AddToCart_AddsItem_WhenNotPresent()
    {
        var logic = new CartLogic();

        var result = logic.AddToCart(1, "Game A", 10.0);

        Assert.IsTrue(result);
        Assert.AreEqual(1, logic.GetCartItems().Count);
        Assert.AreEqual("Game A", logic.GetCartItems()[0].Name);
    }

    [TestMethod]
    public void AddToCart_ReturnsFalse_WhenItemAlreadyExists()
    {
        var logic = new CartLogic();
        logic.AddToCart(1, "Game A", 10.0);

        var result = logic.AddToCart(1, "Game A", 10.0);

        Assert.IsFalse(result);
        Assert.AreEqual(1, logic.GetCartItems().Count);
    }

    [TestMethod]
    public void RemoveFromCart_RemovesByName()
    {
        var logic = new CartLogic();
        logic.AddToCart(1, "Game A", 10.0);
        logic.AddToCart(2, "Game B", 15.0);

        logic.RemoveFromCart("Game A");

        Assert.AreEqual(1, logic.GetCartItems().Count);
        Assert.AreEqual("Game B", logic.GetCartItems()[0].Name);
    }

    [TestMethod]
    public void ClearCart_RemovesAllItems()
    {
        var logic = new CartLogic();
        logic.AddToCart(1, "Game A", 10.0);
        logic.AddToCart(2, "Game B", 15.0);

        logic.ClearCart();

        Assert.AreEqual(0, logic.GetCartItems().Count);
    }

    [TestMethod]
    public void GetTotalPrice_ReturnsSumOfPrices()
    {
        var logic = new CartLogic();
        logic.AddToCart(1, "Game A", 10.0);
        logic.AddToCart(2, "Game B", 15.5);

        var total = logic.GetTotalPrice();

        Assert.AreEqual(25.5, total, 0.001);
    }
}
