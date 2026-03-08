using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Linq;

public class GameAccess
{
    private readonly string _connectionString = AppConfig.ConnectionString;

    public void AddGame(GameModel game)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = @"
            INSERT INTO game (publisher_id, title, description, genre_id, age_rating_id, price, is_active) 
            VALUES (@PublisherId, @Title, @Description, @GenreId, @AgeRatingId, @Price, @IsActive)";
        
        connection.Execute(sql, game);
    }
    public void UpdateGame(GameModel game)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = @"
            UPDATE game SET 
                publisher_id = @PublisherId, 
                title = @Title, 
                description = @Description, 
                genre_id = @GenreId, 
                age_rating_id = @AgeRatingId, 
                price = @Price, 
                is_active = @IsActive 
            WHERE id = @Id";
            
        connection.Execute(sql, game);
    }
    public List<GameModel> SearchByTitle(string searchTerm)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        // ILIKE does case-insensitive searching in Postgres
        string sql = "SELECT * FROM game WHERE title ILIKE @SearchTerm ORDER BY title";
        
        // % symbols so it finds it anywhere in the title
        return connection.Query<GameModel>(sql, new { SearchTerm = $"%{searchTerm}%" }).ToList();
    }

    public void SoftDeleteGame(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = "UPDATE game SET is_active = false WHERE id = @Id";
        connection.Execute(sql, new { Id = id });
    }

    public List<GameModel> GetAllGames()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = "SELECT * FROM game ORDER BY title";
        return connection.Query<GameModel>(sql).ToList();
    }
    public List<GenreModel> GetAllGenres()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        return connection.Query<GenreModel>("SELECT * FROM genre ORDER BY name").ToList();
    }

    public List<AgeRatingModel> GetAllAgeRatings()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        // Ordering by ID so Age Ratings appear in their logical order (E, T, M, A)
        return connection.Query<AgeRatingModel>("SELECT * FROM age_rating ORDER BY id").ToList();
    }

    public List<GameModel> GetActiveGames()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        string sql = "SELECT * FROM game WHERE is_active = true ORDER BY title";
        return connection.Query<GameModel>(sql).ToList();
    }
}