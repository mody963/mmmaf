public class CheckoutLogic
{
    private readonly OrdersAccess _ordersAccess = new OrdersAccess();

    public int Checkout(int customerId, List<CartModel> items)
    {
        if (customerId <= 0)
            throw new ArgumentException("Invalid customer id.");

        if (items == null || items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        double totalPrice = items.Sum(item => item.Price);

        return _ordersAccess.CreateOrder(customerId, totalPrice, items);
    }
}