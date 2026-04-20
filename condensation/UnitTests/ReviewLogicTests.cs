namespace UnitTests;

[TestClass]
public class ReviewLogicTests
{
    [TestMethod]
    public void IsValidRating_ReturnsTrueForRange1To10()
    {
        var logic = new ReviewLogic(new FakeReviewAccess());

        Assert.IsTrue(logic.IsValidRating(1));
        Assert.IsTrue(logic.IsValidRating(10));
    }

    [TestMethod]
    public void IsValidRating_ReturnsFalseOutsideRange()
    {
        var logic = new ReviewLogic(new FakeReviewAccess());

        Assert.IsFalse(logic.IsValidRating(0));
        Assert.IsFalse(logic.IsValidRating(11));
    }

    [TestMethod]
    public void SaveReview_Throws_WhenGameNotOwned()
    {
        var fakeAccess = new FakeReviewAccess { HasPurchased = false };
        var logic = new ReviewLogic(fakeAccess);

        var review = new ReviewModel
        {
            GameId = 1,
            CustomerId = 1,
            Rating = 8,
            Comment = "Great game"
        };

        Assert.ThrowsException<InvalidOperationException>(() => logic.SaveReview(review));
        Assert.AreEqual(0, fakeAccess.UpsertCalls);
    }

    [TestMethod]
    public void SaveReview_Throws_WhenRatingInvalid()
    {
        var fakeAccess = new FakeReviewAccess { HasPurchased = true };
        var logic = new ReviewLogic(fakeAccess);

        var review = new ReviewModel
        {
            GameId = 1,
            CustomerId = 1,
            Rating = 12,
            Comment = "Great game"
        };

        Assert.ThrowsException<InvalidOperationException>(() => logic.SaveReview(review));
        Assert.AreEqual(0, fakeAccess.UpsertCalls);
    }

    [TestMethod]
    public void SaveReview_Throws_WhenCommentInvalid()
    {
        var fakeAccess = new FakeReviewAccess { HasPurchased = true };
        var logic = new ReviewLogic(fakeAccess);

        var review = new ReviewModel
        {
            GameId = 1,
            CustomerId = 1,
            Rating = 8,
            Comment = "  "
        };

        Assert.ThrowsException<InvalidOperationException>(() => logic.SaveReview(review));
        Assert.AreEqual(0, fakeAccess.UpsertCalls);
    }

    [TestMethod]
    public void SaveReview_Upserts_WhenValidAndOwned()
    {
        var fakeAccess = new FakeReviewAccess { HasPurchased = true };
        var logic = new ReviewLogic(fakeAccess);

        var review = new ReviewModel
        {
            GameId = 9,
            CustomerId = 4,
            Rating = 7,
            Comment = "  Nice story and music  "
        };

        logic.SaveReview(review);

        Assert.AreEqual(1, fakeAccess.UpsertCalls);
        Assert.IsNotNull(fakeAccess.LastSavedReview);
        Assert.AreEqual("Nice story and music", fakeAccess.LastSavedReview!.Comment);
        Assert.AreEqual(9, fakeAccess.LastSavedReview.GameId);
        Assert.AreEqual(4, fakeAccess.LastSavedReview.CustomerId);
        Assert.AreEqual(7, fakeAccess.LastSavedReview.Rating);
    }

    [TestMethod]
    public void DeleteReviewWithAuth_Admin_CanDeleteAnyReview()
    {
        var fakeAccess = new FakeReviewAccess 
        { 
            ReviewToReturn = new ReviewModel { Id = 5, GameId = 1, CustomerId = 2 }
        };
        var fakeGameLogic = new FakeGameLogic 
        { 
            GameToReturn = new GameModel { Id = 1, PublisherId = 3 } 
        };
        var logic = new ReviewLogic(fakeAccess, fakeGameLogic);

        // Admin (role=1) deleting a review
        logic.DeleteReviewWithAuth(userId: 99, userRole: (int)AccountRoles.Admin, reviewId: 5, gameId: 1);

        Assert.AreEqual(1, fakeAccess.DeleteCalls);
    }

    [TestMethod]
    public void DeleteReviewWithAuth_Customer_CanDeleteOwnReview()
    {
        var fakeAccess = new FakeReviewAccess 
        { 
            ReviewToReturn = new ReviewModel { Id = 5, GameId = 1, CustomerId = 10 }
        };
        var fakeGameLogic = new FakeGameLogic();
        var logic = new ReviewLogic(fakeAccess, fakeGameLogic);

        // Customer (role=0) deleting their own review
        logic.DeleteReviewWithAuth(userId: 10, userRole: (int)AccountRoles.Customer, reviewId: 5, gameId: 1);

        Assert.AreEqual(1, fakeAccess.DeleteCalls);
    }

    [TestMethod]
    public void DeleteReviewWithAuth_Customer_CannotDeleteOthersReview()
    {
        var fakeAccess = new FakeReviewAccess 
        { 
            ReviewToReturn = new ReviewModel { Id = 5, GameId = 1, CustomerId = 10 }
        };
        var fakeGameLogic = new FakeGameLogic();
        var logic = new ReviewLogic(fakeAccess, fakeGameLogic);

        // Customer (role=0) trying to delete someone else's review
        var ex = Assert.ThrowsException<UnauthorizedAccessException>(() =>
            logic.DeleteReviewWithAuth(userId: 20, userRole: (int)AccountRoles.Customer, reviewId: 5, gameId: 1)
        );

        Assert.AreEqual("You can only delete your own reviews.", ex.Message);
        Assert.AreEqual(0, fakeAccess.DeleteCalls);
    }

    [TestMethod]
    public void DeleteReviewWithAuth_Publisher_CanDeleteReviewFromOwnGame()
    {
        var fakeAccess = new FakeReviewAccess 
        { 
            ReviewToReturn = new ReviewModel { Id = 5, GameId = 1, CustomerId = 10 }
        };
        var fakeGameLogic = new FakeGameLogic 
        { 
            GameToReturn = new GameModel { Id = 1, PublisherId = 99 } 
        };
        var logic = new ReviewLogic(fakeAccess, fakeGameLogic);

        // Publisher (role=2) deleting review from their own game
        logic.DeleteReviewWithAuth(userId: 99, userRole: (int)AccountRoles.Publisher, reviewId: 5, gameId: 1);

        Assert.AreEqual(1, fakeAccess.DeleteCalls);
    }

    [TestMethod]
    public void DeleteReviewWithAuth_Publisher_CannotDeleteReviewFromOthersGame()
    {
        var fakeAccess = new FakeReviewAccess 
        { 
            ReviewToReturn = new ReviewModel { Id = 5, GameId = 1, CustomerId = 10 }
        };
        var fakeGameLogic = new FakeGameLogic 
        { 
            GameToReturn = new GameModel { Id = 1, PublisherId = 99 } 
        };
        var logic = new ReviewLogic(fakeAccess, fakeGameLogic);

        // Publisher (role=2) trying to delete review from someone else's game
        var ex = Assert.ThrowsException<UnauthorizedAccessException>(() =>
            logic.DeleteReviewWithAuth(userId: 50, userRole: (int)AccountRoles.Publisher, reviewId: 5, gameId: 1)
        );

        Assert.AreEqual("You can only delete reviews from your own published games.", ex.Message);
        Assert.AreEqual(0, fakeAccess.DeleteCalls);
    }

    [TestMethod]
    public void DeleteReviewWithAuth_Throws_WhenReviewNotFound()
    {
        var fakeAccess = new FakeReviewAccess 
        { 
            ReviewToReturn = null
        };
        var fakeGameLogic = new FakeGameLogic();
        var logic = new ReviewLogic(fakeAccess, fakeGameLogic);

        // Try to delete a non-existent review
        var ex = Assert.ThrowsException<InvalidOperationException>(() =>
            logic.DeleteReviewWithAuth(userId: 10, userRole: (int)AccountRoles.Customer, reviewId: 999, gameId: 1)
        );

        Assert.AreEqual("Review not found.", ex.Message);
        Assert.AreEqual(0, fakeAccess.DeleteCalls);
    }

    [TestMethod]
    public void DeleteReviewWithAuth_Publisher_Throws_WhenGameNotFound()
    {
        var fakeAccess = new FakeReviewAccess 
        { 
            ReviewToReturn = new ReviewModel { Id = 5, GameId = 1, CustomerId = 10 }
        };
        var fakeGameLogic = new FakeGameLogic 
        { 
            GameToReturn = null 
        };
        var logic = new ReviewLogic(fakeAccess, fakeGameLogic);

        // Publisher trying to delete review from non-existent game
        var ex = Assert.ThrowsException<UnauthorizedAccessException>(() =>
            logic.DeleteReviewWithAuth(userId: 99, userRole: (int)AccountRoles.Publisher, reviewId: 5, gameId: 999)
        );

        Assert.AreEqual("You can only delete reviews from your own published games.", ex.Message);
        Assert.AreEqual(0, fakeAccess.DeleteCalls);
    }

    private class FakeReviewAccess : IReviewAccess
    {
        public bool HasPurchased { get; set; }
        public int UpsertCalls { get; private set; }
        public int DeleteCalls { get; private set; }
        public ReviewModel? LastSavedReview { get; private set; }
        public ReviewModel? ReviewToReturn { get; set; }

        public List<GameModel> GetOwnedGamesByCustomerId(int customerId)
        {
            return new List<GameModel>();
        }

        public bool HasPurchasedGame(int customerId, int gameId)
        {
            return HasPurchased;
        }

        public List<ReviewModel> GetReviewsForGame(int gameId)
        {
            return new List<ReviewModel>();
        }

        public ReviewModel? GetCustomerReviewForGame(int customerId, int gameId)
        {
            return null;
        }

        public void UpsertReview(ReviewModel review)
        {
            UpsertCalls++;
            LastSavedReview = review;
        }

        public void DeleteReview(int reviewId, int gameId)
        {
            DeleteCalls++;
        }

        public List<ReviewModel> GetReviewsByPublisherId(int publisherId)
        {
            return new List<ReviewModel>();
        }

        public ReviewModel? GetReviewById(int reviewId)
        {
            return ReviewToReturn;
        }

        public List<ReviewModel> GetAllReviewsForGameAdmin(int gameId)
        {
            return new List<ReviewModel>();
        }
    }

    private class FakeGameLogic : IGameLogic
    {
        public GameModel? GameToReturn { get; set; }

        public GameModel? GetGameById(int id)
        {
            return GameToReturn;
        }
    }
}
