public class ReviewLogic
{
    private readonly IReviewAccess _reviewAccess;

    public ReviewLogic()
    {
        _reviewAccess = new ReviewAccess();
    }

    public ReviewLogic(IReviewAccess reviewAccess)
    {
        _reviewAccess = reviewAccess;
    }

    public List<GameModel> GetOwnedGames(int customerId)
    {
        if (customerId <= 0)
            return new List<GameModel>();

        return _reviewAccess.GetOwnedGamesByCustomerId(customerId);
    }

    public List<ReviewModel> GetReviewsForGame(int gameId)
    {
        if (gameId <= 0)
            return new List<ReviewModel>();

        return _reviewAccess.GetReviewsForGame(gameId);
    }

    public ReviewModel? GetCustomerReviewForGame(int customerId, int gameId)
    {
        if (customerId <= 0 || gameId <= 0)
            return null;

        return _reviewAccess.GetCustomerReviewForGame(customerId, gameId);
    }

    public bool CanCustomerReviewGame(int customerId, int gameId)
    {
        if (customerId <= 0 || gameId <= 0)
            return false;

        return _reviewAccess.HasPurchasedGame(customerId, gameId);
    }

    public bool IsValidRating(int rating)
    {
        return rating >= 1 && rating <= 10;
    }

    public bool IsValidComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return false;

        return comment.Trim().Length >= 3;
    }

    public void SaveReview(ReviewModel review)
    {
        if (!CanCustomerReviewGame(review.CustomerId, review.GameId))
            throw new InvalidOperationException("This game is not owned by the customer.");

        if (!IsValidRating(review.Rating))
            throw new InvalidOperationException("Rating must be between 1 and 10.");

        if (!IsValidComment(review.Comment))
            throw new InvalidOperationException("Review comment must contain at least 3 characters.");

        review.Comment = review.Comment.Trim();
        _reviewAccess.UpsertReview(review);
    }
}
