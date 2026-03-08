public class GameModel
{
    public int Id {get; set;}
    public int PublisherId {get; set;}
    public string Title {get; set;}
    public string Description{get; set;}
    public int GenreId {get; set;}
    public int AgeRatingId{get; set;}
    public decimal Price {get; set;}
    public bool IsActive{get; set;} = true;

}