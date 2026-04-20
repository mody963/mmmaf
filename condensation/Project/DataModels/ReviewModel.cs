// public class ReviewModel
// {
//     public int Id { get; set; }
//     public int GameId { get; set; }
//     public int CustomerId { get; set; }
//     public int Rating { get; set; }
//     public string Comment { get; set; } = string.Empty;
//     public DateTime CreatedAt { get; set; }
//     public string ReviewerName { get; set; } = string.Empty;
// }

public class ReviewModel
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int CustomerId { get; set; }
    
    // Updated fields based on requirements
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string Pros { get; set; } = string.Empty;
    public string Cons { get; set; } = string.Empty;
    public int Rating { get; set; } // Must be 1-5
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    
    // New field for the Admin requirement
    public bool IsHidden { get; set; } = false; 
}


// public class ReviewModel
// {
//     public int GameId { get; set; }
//     public int CustomerId { get; set; }
//     public int Rating { get; set; }
//     public string Comment { get; set; }
//     public DateTime CreatedAt { get; set; }
// }