namespace UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

[TestClass]
public class GameLogicTest
{
    [TestInitialize]
    public void Setup()
    {
        AppConfig.PostgresConnectionString = "Host=localhost;Port=5432;Database=Condensation;Username=menno;Password=test12345";

        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    [TestMethod]
    public void SearchAndDisplayGames_Valid_Game_return()
    {
        var gameLogic = new GameLogic();

        // take an existing game from the database to ensure we search for something that actually exists
        var existing = gameLogic.GetActiveGames().FirstOrDefault();
        Assert.IsNotNull(existing, "No active games in the database to search for.");

        var result = gameLogic.SearchGamesByTitle(existing!.Title);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Any(g => g.Id == existing.Id), "The searched game is not in the results.");
    }

    [TestMethod]
    public void SearchAndDisplayGames_Partial_Title_return()
    {
        var gameLogic = new GameLogic();

        var existing = gameLogic.GetActiveGames().FirstOrDefault();
        Assert.IsNotNull(existing, "No active games in the database to search for.");

        // take the first few letters of an existing title
        string partial = existing!.Title.Length >= 3
            ? existing.Title.Substring(0, 3)
            : existing.Title;

        var result = gameLogic.SearchGamesByTitle(partial);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0, "Searching for a partial existing title returned nothing.");
    }

    [TestMethod]
    public void SearchAndDisplayGames_No_Game_return()
    {
        var gameLogic = new GameLogic();

        var result = gameLogic.SearchGamesByTitle(""); // empty search term

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}