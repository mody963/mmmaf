using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class OrderDocumentModel
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("orderNumber")]
    public string OrderNumber { get; set; }

    [BsonElement("customerId")]
    public int CustomerId { get; set; }

    [BsonElement("orderDate")]
    public DateTime OrderDate { get; set; }

    [BsonElement("items")]
    public List<OrderItemModel> Items { get; set; }

    [BsonElement("shippingAddress")]
    public string ShippingAddress { get; set; }

    [BsonElement("paymentStatus")]
    public string PaymentStatus { get; set; }

    [BsonElement("orderStatus")]
    public string OrderStatus { get; set; }

    [BsonElement("totalPrice")]
    public double TotalPrice { get; set; }

    [BsonElement("statusHistory")]
    public List<OrderStatusHistoryModel> StatusHistory { get; set; }

    public OrderDocumentModel()
    {
        Items = new List<OrderItemModel>();
        StatusHistory = new List<OrderStatusHistoryModel>();
        OrderDate = DateTime.Now;
    }

    public OrderDocumentModel(int customerId, string orderNumber, string shippingAddress, double totalPrice)
    {
        Id = ObjectId.GenerateNewId();
        CustomerId = customerId;
        OrderNumber = orderNumber;
        ShippingAddress = shippingAddress;
        TotalPrice = totalPrice;
        OrderDate = DateTime.Now;
        Items = new List<OrderItemModel>();
        StatusHistory = new List<OrderStatusHistoryModel>();
        PaymentStatus = "Pending";
        OrderStatus = "Created";
    }
}
