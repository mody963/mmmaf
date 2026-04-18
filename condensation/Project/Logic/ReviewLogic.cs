public class ReviewLogic
{
    private readonly IReviewAccess _reviewAccess;
    public ReviewLogic()
    {
        _reviewAccess = new ReviewAccess(); // Redis
    }

    public ReviewLogic(IReviewAccess reviewAccess)
    {
        _reviewAccess = reviewAccess;
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

    public bool IsValidRating(int rating)
    {
        return rating >= 1 && rating <= 5;
    }

    public bool IsValidComment(string? comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return false;

        return comment.Trim().Length >= 3;
    }

    public void SaveReview(ReviewModel review)
    {
        if (!IsValidRating(review.Rating))
            throw new InvalidOperationException("Rating must be between 1 and 5.");

        if (!IsValidComment(review.Comment))
            throw new InvalidOperationException("Review comment must contain at least 3 characters.");

        review.Comment = review.Comment.Trim();
        _reviewAccess.UpsertReview(review);
    }
    public void DeleteReview(ReviewModel review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        _reviewAccess.DeleteReview(review.Id, review.GameId);
    }

        public List<ReviewModel> GetPublisherReviews(int publisherId)
    {
        return _reviewAccess.GetReviewsByPublisherId(publisherId);
    }

    public bool IsValidTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;

        return title.Trim().Length >= 3;
    }

    public bool IsValidPros(string? pros)
    {
        if (string.IsNullOrWhiteSpace(pros))
            return false;

        return pros.Trim().Length >= 3;
    }

    public bool IsValidCons(string? cons)
    {
        if (string.IsNullOrWhiteSpace(cons))
            return false;

        return cons.Trim().Length >= 3;
    }

    public double GetAverageRatingForGame(int gameId)
    {
        var reviews = GetReviewsForGame(gameId); // Takes only visible reviews, filter is handled in access layer

        if (reviews.Count == 0)
            return 0;

        return reviews.Average(r => r.Rating);
    }
}
