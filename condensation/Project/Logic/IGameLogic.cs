
public interface IGameLogic
{
    void AddGame(GameModel game);
    void UpdateGame(GameModel game);
    List<GameModel> SearchGamesByTitle(string title);
    void SoftDeleteGame(int id);
    List<GameModel> GetAllGames();
    List<GenreModel> GetAllGenres();
    List<AgeRatingModel> GetAllAgeRatings();
    List<GameModel> GetActiveGames();
    List<GameModel> GetGamesByGenre(int genreId);
    GameModel? GetGameById(int id);
}