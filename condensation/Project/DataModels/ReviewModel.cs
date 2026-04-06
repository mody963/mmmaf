public class ReviewModel
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int CustomerId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
}
