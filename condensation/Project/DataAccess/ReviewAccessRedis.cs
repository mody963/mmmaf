using StackExchange.Redis;
using System.Text.Json;

public class ReviewAccess : IReviewAccess
{
    private readonly IDatabase _db;
    private readonly OrdersAccess _ordersAccess;

    public ReviewAccess()
    {
        var redis = ConnectionMultiplexer.Connect(AppConfig.RedisConnectionString);
        _db = redis.GetDatabase();
        _ordersAccess = new OrdersAccess();
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
        string gameSortedKey = $"game:{review.GameId}:reviews:sorted"; //sorted set key
        string publisherKey = $"publisher:reviews"; // helps with the publisher requirement later

        string json = JsonSerializer.Serialize(review);

        // Save review by ID
        _db.StringSet(reviewKey, json);

        // Index it under the game so we can easily fetch all reviews for a game
        _db.SetAdd(gameKey, review.Id);

        // add to sorted set using timestamp as score
        _db.SortedSetAdd(gameSortedKey, review.Id, review.CreatedAt.Ticks);
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

                if (review != null && review.CustomerId == customerId && !review.IsHidden)
                    return review;
            }
        }

        return null;
    }
    
    public List<ReviewModel> GetReviewsForGame(int gameId)
    {
        // string gameKey = $"game:{gameId}:reviews";
        // var reviewIds = _db.SetMembers(gameKey);
        string gameSortedKey = $"game:{gameId}:reviews:sorted";
        var reviewIds = _db.SortedSetRangeByRank(gameSortedKey, 0, -1, Order.Descending);
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
        //return reviews.OrderByDescending(r => r.CreatedAt).ToList();
        return reviews;
    }

    public void DeleteReview(int reviewId, int gameId)
    {
        string reviewKey = $"review:{reviewId}";
        string gameKey = $"game:{gameId}:reviews";
        string gameSortedKey = $"game:{gameId}:reviews:sorted";

        // 1. Remove the JSON object
        _db.KeyDelete(reviewKey);

        // 2. Remove the ID from the game's index list
        _db.SetRemove(gameKey, reviewId);
        // 3 delete the id from sorted set
        _db.SortedSetRemove(gameSortedKey, reviewId);
    }

    public bool HasPurchasedGame(int customerId, int gameId)
    {
        return _ordersAccess.HasPurchasedGame(customerId, gameId);
    }

    public List<GameModel> GetOwnedGamesByCustomerId(int customerId)
    {
        throw new NotImplementedException();
    }

    public List<ReviewModel> GetReviewsByPublisherId(int publisherId)
    {
        return new List<ReviewModel>();
    }
    public ReviewModel? GetReviewById(int reviewId)
    {
        var value = _db.StringGet($"review:{reviewId}");
        
        if (value.IsNullOrEmpty) 
            return null;
            
        return JsonSerializer.Deserialize<ReviewModel>(value!);
    }

    // no dilter for hidden games
    public List<ReviewModel> GetAllReviewsForGameAdmin(int gameId)
    {
        string gameSortedKey = $"game:{gameId}:reviews:sorted";
        var reviewIds = _db.SortedSetRangeByRank(gameSortedKey, 0, -1, Order.Descending);
        List<ReviewModel> reviews = new();

        foreach (var id in reviewIds)
        {
            var value = _db.StringGet($"review:{id}");
            if (!value.IsNullOrEmpty)
            {
                var review = JsonSerializer.Deserialize<ReviewModel>(value!);
                
                if (review != null) 
                {
                    reviews.Add(review);
                }
            }
        }

        return reviews;
    }
}