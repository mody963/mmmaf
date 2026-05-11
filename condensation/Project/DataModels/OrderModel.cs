public class OrderModel
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public double TotalPrice { get; set; }
    public List<OrderGameModel> Games { get; set; } = new List<OrderGameModel>();
}
