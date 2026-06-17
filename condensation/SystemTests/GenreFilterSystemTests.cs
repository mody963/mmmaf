using Xunit;
using System.Collections.Generic;

public class GenreFilterSystemTests
{
    private const string PostgresConn = "Host=localhost;Port=5432;Database=Condensation;Username=menno;Password=test12345";

    public GenreFilterSystemTests()
    {
        AppConfig.PostgresConnectionString = PostgresConn;
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    [Fact]
    public void FilterByGenre_ReturnsOnlyActiveGamesOfThatGenre()
    {
        var logic = new GameLogic();

        var genres = logic.GetAllGenres();
        Assert.True(genres.Count > 0, "There are no genres in the database to filter by.");

        GenreModel? genreWithGames = null;
        List<GameModel> games = new();

        foreach (var genre in genres)
        {
            var result = logic.GetGamesByGenre(genre.Id);
            if (result.Count > 0)
            {
                genreWithGames = genre;
                games = result;
                break;
            }
        }

        Assert.NotNull(genreWithGames);

        foreach (var game in games)
        {
            Assert.Equal(genreWithGames!.Id, game.GenreId);
            Assert.True(game.IsActive, "Inactive games should not be displayed.");
        }
    }

    [Fact]
    public void FilterByGenre_ReturnsEmpty_ForNonExistentGenre()
    {
        var logic = new GameLogic();
        var result = logic.GetGamesByGenre(int.MaxValue);
        Assert.Empty(result);
    }
}