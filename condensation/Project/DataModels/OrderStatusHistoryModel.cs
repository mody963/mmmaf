public class OrderStatusHistoryModel
{
    public string Status { get; set; }
    public DateTime StatusChangedAt { get; set; }

    public OrderStatusHistoryModel(string status, DateTime statusChangedAt)
    {
        Status = status;
        StatusChangedAt = statusChangedAt;
    }
}
