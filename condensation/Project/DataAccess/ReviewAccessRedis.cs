using StackExchange.Redis;
using System.Text.Json;

public class ReviewAccess : IReviewAccess
{
    private readonly IDatabase _db;

    public ReviewAccess()
    {
        var redis = ConnectionMultiplexer.Connect(AppConfig.RedisConnectionString);
        _db = redis.GetDatabase();
    }

    // STEP 1: Save (Upsert)
    public void UpsertReview(ReviewModel review)
    {
        // 1. Generate a new ID if this is a brand new review
        if (review.Id == 0)
        {
            // Redis  auto-increment this counter
            review.Id = (int)_db.StringIncrement("review:id_counter");
            review.CreatedAt = DateTime.UtcNow;
        }

        string reviewKey = $"review:{review.Id}";
        string gameKey = $"game:{review.GameId}:reviews";
        string publisherKey = $"publisher:reviews"; // helps with the publisher requirement later

        string json = JsonSerializer.Serialize(review);

        // Save review by ID
        _db.StringSet(reviewKey, json);

        // Index it under the game so we can easily fetch all reviews for a game
        _db.SetAdd(gameKey, review.Id);
    }

    public ReviewModel? GetCustomerReviewForGame(int customerId, int gameId)
    {
        string gameKey = $"game:{gameId}:reviews";

        var reviewIds = _db.SetMembers(gameKey);

        foreach (var id in reviewIds)
        {
            var value = _db.StringGet($"review:{id}");

            if (!value.IsNullOrEmpty)
            {
                var review = JsonSerializer.Deserialize<ReviewModel>(value!);

                if (review != null && review.CustomerId == customerId)
                    return review;
            }
        }

        return null;
    }

    // TEMP (not implemented yet)    
    public List<ReviewModel> GetReviewsForGame(int gameId)
    {
        string gameKey = $"game:{gameId}:reviews";
        var reviewIds = _db.SetMembers(gameKey);
        List<ReviewModel> reviews = new();

        foreach (var id in reviewIds)
        {
            var value = _db.StringGet($"review:{id}");
            if (!value.IsNullOrEmpty)
            {
                var review = JsonSerializer.Deserialize<ReviewModel>(value!);
                
                // Requirement: Do not show hidden reviews to customers
                if (review != null && !review.IsHidden) 
                {
                    reviews.Add(review);
                }
            }
        }

        // Requirement: Sort by date, newest first
        return reviews.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public void DeleteReview(int reviewId, int gameId)
    {
        string reviewKey = $"review:{reviewId}";
        string gameKey = $"game:{gameId}:reviews";

        // 1. Remove the JSON object
        _db.KeyDelete(reviewKey);

        // 2. Remove the ID from the game's index list
        _db.SetRemove(gameKey, reviewId);
    }

    public bool HasPurchasedGame(int customerId, int gameId)
    {
        throw new NotImplementedException(); // still using Postgres later
    }

    public List<GameModel> GetOwnedGamesByCustomerId(int customerId)
    {
        throw new NotImplementedException();
    }

    public List<ReviewModel> GetReviewsByPublisherId(int publisherId)
    {
        return new List<ReviewModel>();
    }
}