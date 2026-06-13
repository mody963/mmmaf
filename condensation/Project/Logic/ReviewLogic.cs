public class ReviewLogic
{
    private readonly IReviewAccess _reviewAccess;
    private readonly IGameLogic _gameLogic;
    private readonly PermissionsLogic _permissions = new PermissionsLogic();
    private readonly CustomersLogic _customers = new CustomersLogic();
    private readonly PublisherLogic _publishers = new PublisherLogic();

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
        var reviews = GetReviewsForGame(gameId);

        if (reviews.Count == 0)
            return 0;

        return reviews.Average(r => r.Rating);
    }

    public List<ReviewModel> GetAllReviewsForGameAdmin(int gameId)
    {
        if (gameId <= 0)
            return new List<ReviewModel>();

        return _reviewAccess.GetAllReviewsForGameAdmin(gameId);
    }

    public void ToggleReviewVisibility(int reviewId)
    {
        var review = _reviewAccess.GetReviewById(reviewId);

        if (review != null)
        {
            review.IsHidden = !review.IsHidden;
            _reviewAccess.UpsertReview(review);
        }
    }

    public void DeleteReview(int reviewId, int gameId)
    {
        _reviewAccess.DeleteReview(reviewId, gameId);
    }

    // accountId = the logged in account id, permissions are checked by account
    public void DeleteReviewWithAuth(int accountId, int reviewId, int gameId)
    {
        if (_permissions.HasPermission(accountId, Permissions.ReviewsDeleteAny))
        {
            DeleteReview(reviewId, gameId);
            return;
        }

        var review = _reviewAccess.GetReviewById(reviewId);
        if (review == null)
            throw new InvalidOperationException("Review not found.");

        if (_permissions.HasPermission(accountId, Permissions.ReviewsDeleteOwn))
        {
            var customer = _customers.GetByAccountId(accountId);
            if (customer != null && review.CustomerId == customer.Id)
            {
                DeleteReview(reviewId, gameId);
                return;
            }
        }

        if (_permissions.HasPermission(accountId, Permissions.ReviewsDeleteOwnGames))
        {
            var game = _gameLogic.GetGameById(gameId);
            var publisher = _publishers.GetByAccountId(accountId);
            if (game != null && publisher != null && game.PublisherId == publisher.Id)
            {
                DeleteReview(reviewId, gameId);
                return;
            }
        }

        throw new UnauthorizedAccessException("You do not have permission to delete this review.");
    }
}