using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserActionLogModel
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string ActionType { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }

    public string ObjectType { get; set; } = "";
    public string? ObjectId { get; set; }

    public BsonDocument Details { get; set; } = new BsonDocument();
}