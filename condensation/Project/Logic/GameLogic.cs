using System.Collections.Generic;

public class GameLogic : IGameLogic
{
    private readonly GameAccess _gameAccess = new GameAccess();

    public void AddGame(GameModel game)
    {
        if (string.IsNullOrWhiteSpace(game.Title)) return;
        _gameAccess.AddGame(game);
    }

    public void UpdateGame(GameModel game)
    {
        _gameAccess.UpdateGame(game);
    }

    public List<GameModel> SearchGamesByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return new List<GameModel>();
        return _gameAccess.SearchByTitle(title);
    }

    public void SoftDeleteGame(int id)
    {
        _gameAccess.SoftDeleteGame(id);
    }

    public List<GameModel> GetAllGames()
    {
        return _gameAccess.GetAllGames();
    }
    public List<GenreModel> GetAllGenres()
    {
        return _gameAccess.GetAllGenres();
    }

    public List<AgeRatingModel> GetAllAgeRatings()
    {
        return _gameAccess.GetAllAgeRatings();
    }

    public List<GameModel> GetActiveGames()
    {
        return _gameAccess.GetActiveGames();
    }

    public List<GameModel> GetGamesByGenre(int genreId)
    {
        if (genreId <= 0) return new List<GameModel>();
        
        return _gameAccess.GetGamesByGenre(genreId);
    }

    public GameModel GetGameById(int id)
    {
        if (id <= 0) return null;
        return _gameAccess.GetGameById(id);
    }

    
}