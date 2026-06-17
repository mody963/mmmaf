using Xunit;
using MongoDB.Driver;

public class LoggingSystemTests : IDisposable
{
    private const string MongoConn = "mongodb://hro_gebruiker:hetismongodb@pg-hro.crazyelectron.io:27017/?authSource=admin";
    private const string MongoDbName = "Condensation";

    private readonly string _marker = "test-" + Guid.NewGuid();

    public LoggingSystemTests()
    {
        AppConfig.MongoDbConnectionString = MongoConn;
        AppConfig.MongoDbDatabaseName = MongoDbName;
    }

    public void Dispose() => DeleteByMarker(_marker);

    [Fact]
    public void Log_AdminAction_IsStoredInMongo_WithUserAndSession()
    {
        // Simulate that an admin user is logged in
        CurrentUserModel.CurrentUser = new AccountModel
        {
            Id = 1, Email = "admin@test", FirstName = "Test", LastName = "Admin",
            Password = "x", Role = 1, IsActive = true
        };

        UserActionLogger.Log(actionType: "test_admin_action", objectType: "test", objectId: _marker);

        var doc = FindByMarker(_marker);
        Assert.NotNull(doc);
        Assert.Equal("test_admin_action", doc!.ActionType);
        Assert.Equal(1, doc.UserId);
        Assert.False(string.IsNullOrWhiteSpace(doc.SessionId));
    }

    [Fact]
    public void Log_AnonymousUser_IsStored_WithoutUserId_ButWithSession()
    {
        CurrentUserModel.CurrentUser = null; // Nobody is logged in

        UserActionLogger.Log(actionType: "test_anon_action", objectType: "test", objectId: _marker);

        var doc = FindByMarker(_marker);
        Assert.NotNull(doc);
        Assert.Null(doc!.UserId);                                // No userid for anonymous user
        Assert.False(string.IsNullOrWhiteSpace(doc.SessionId));  // SessionId should still be theree
    }

    private static IMongoCollection<UserActionLogModel> Logs()
    {
        var client = new MongoClient(AppConfig.MongoDbConnectionString);
        return client.GetDatabase(AppConfig.MongoDbDatabaseName)
                     .GetCollection<UserActionLogModel>("user_action_logs");
    }

    private static UserActionLogModel? FindByMarker(string marker) =>
        Logs().Find(Builders<UserActionLogModel>.Filter.Eq(x => x.ObjectId, marker)).FirstOrDefault();

    private static void DeleteByMarker(string marker) =>
        Logs().DeleteMany(Builders<UserActionLogModel>.Filter.Eq(x => x.ObjectId, marker));
}