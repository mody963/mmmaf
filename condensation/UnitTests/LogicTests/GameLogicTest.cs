namespace UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;


[TestClass]
public class GameLogicTest
{
    [TestMethod]
    public void SearchAndDisplayGames_Valid_Game_return()
    {
        var gameLogic = new GameLogic();
        string validTitle = "Elden Ring";
        
        var result = gameLogic.SearchGamesByTitle(validTitle);
        
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
    }
    [TestMethod]
    public void SearchAndDisplayGames_No_Game_return()
    {
        var gameLogic = new GameLogic();
        string invalidTitle = "";
        

        var result = gameLogic.SearchGamesByTitle(invalidTitle);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestInitialize]
    public void Setup()
    {
        
        AppConfig.PostgresConnectionString =
            "Host=localhost;Port=5432;Database=Condensation;Username=postgres;Password=xxx";
    }

    [TestMethod]
    public void SearchAndDisplayGames_Partial_Title_return()
    {
        var gameLogic = new GameLogic();

        var result = gameLogic.SearchGamesByTitle("lden");

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
    }

}
