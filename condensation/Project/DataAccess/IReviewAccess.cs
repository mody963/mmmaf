public interface IReviewAccess
{
    List<GameModel> GetOwnedGamesByCustomerId(int customerId);
    bool HasPurchasedGame(int customerId, int gameId);
    List<ReviewModel> GetReviewsForGame(int gameId);
    ReviewModel? GetCustomerReviewForGame(int customerId, int gameId);
    void UpsertReview(ReviewModel review);
    void DeleteReview(int reviewId, int gameId);
    List<ReviewModel> GetReviewsByPublisherId(int publisherId);
}
