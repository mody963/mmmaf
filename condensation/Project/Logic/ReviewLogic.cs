public class ReviewLogic
{
    private readonly IReviewAccess _reviewAccess;
    private readonly IGameLogic _gameLogic;

    public ReviewLogic()
    {
        _reviewAccess = new ReviewAccess(); // Redis
        _gameLogic = new GameLogic();
    }

    public ReviewLogic(IReviewAccess reviewAccess)
    {
        _reviewAccess = reviewAccess;
        _gameLogic = new GameLogic();
    }

    public ReviewLogic(IReviewAccess reviewAccess, IGameLogic gameLogic)
    {
        _reviewAccess = reviewAccess;
        _gameLogic = gameLogic;
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
        if (!IsValidRating(review.Rating))
            throw new InvalidOperationException("Rating must be between 1 and 10.");

        if (!IsValidComment(review.Comment))
            throw new InvalidOperationException("Review comment must contain at least 3 characters.");

        if (!_reviewAccess.HasPurchasedGame(review.CustomerId, review.GameId))
            throw new InvalidOperationException("You must own this game to leave a review.");

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

    public void DeleteReview(int reviewId, int gameId)
    {
        _reviewAccess.DeleteReview(reviewId, gameId);
    }

    /// <summary>
    /// Validates if a user can delete a review based on their role and permissions.
    /// </summary>
    /// <param name="userId">Current user's ID</param>
    /// <param name="userRole">Current user's role (0=Customer, 1=Admin, 2=Publisher)</param>
    /// <param name="reviewId">Review to delete</param>
    /// <param name="gameId">Game the review belongs to</param>
    /// <exception cref="UnauthorizedAccessException">Thrown if user doesn't have permission</exception>
    public void DeleteReviewWithAuth(int userId, int userRole, int reviewId, int gameId)
    {
        // Admin (role 1) can delete any review
        if (userRole == (int)AccountRoles.Admin)
        {
            DeleteReview(reviewId, gameId);
            return;
        }

        // Get the review to check ownership
        var review = _reviewAccess.GetReviewById(reviewId);
        if (review == null)
            throw new InvalidOperationException("Review not found.");

        // Customer (role 0) can only delete their own reviews
        if (userRole == (int)AccountRoles.Customer)
        {
            if (review.CustomerId != userId)
                throw new UnauthorizedAccessException("You can only delete your own reviews.");

            DeleteReview(reviewId, gameId);
            return;
        }

        // Publisher (role 2) can only delete reviews from their own published games
        if (userRole == (int)AccountRoles.Publisher)
        {
            var game = _gameLogic.GetGameById(gameId);
            if (game == null || game.PublisherId != userId)
                throw new UnauthorizedAccessException("You can only delete reviews from your own published games.");

            DeleteReview(reviewId, gameId);
            return;
        }

        throw new UnauthorizedAccessException("Invalid user role.");
    }
}
