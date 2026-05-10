using MongoDB.Driver;

public class UserActionLogAccess
{
    private readonly IMongoCollection<UserActionLogModel> _collection;

    public UserActionLogAccess()
    {
        var client = new MongoClient(AppConfig.MongoDbConnectionString);
        var database = client.GetDatabase(AppConfig.MongoDbDatabaseName);
        _collection = database.GetCollection<UserActionLogModel>("user_action_logs");
    }

    public void Insert(UserActionLogModel log)
    {
        _collection.InsertOne(log);
    }
}
