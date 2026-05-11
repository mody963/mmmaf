public class RevenueResult
{
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class PriceChartItem
{
    public int Id { get; set; }
    public string GameName { get; set; } = "";
    public decimal Price { get; set; }
}

public class SoldChartItem
{
    public string Name { get; set; } = "";
    public int SoldCopies { get; set; }
}