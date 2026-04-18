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
    public List<ReviewModel> GetAllReviewsForGameAdmin(int gameId)
    {
        if (gameId <= 0) return new List<ReviewModel>();
        
        return _reviewAccess.GetAllReviewsForGameAdmin(gameId);
    }

    public void ToggleReviewVisibility(int reviewId)
    {
        // get the review
        var review = _reviewAccess.GetReviewById(reviewId);
        
        if (review != null)
        {
            // flip the boolean
            review.IsHidden = !review.IsHidden;
            
            // save to redis/ overwrite cause its same
            _reviewAccess.UpsertReview(review);
        }
    }
}
