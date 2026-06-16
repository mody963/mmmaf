namespace UnitTests;

[TestClass]
public class ReviewLogicTests
{
    [TestMethod]
    public void IsValidRating_ReturnsTrueForRange1To5()
    {
        var logic = new ReviewLogic(new FakeReviewAccess());

        Assert.IsTrue(logic.IsValidRating(1));
        Assert.IsTrue(logic.IsValidRating(5));
    }

    [TestMethod]
    public void IsValidRating_ReturnsFalseOutsideRange()
    {
        var logic = new ReviewLogic(new FakeReviewAccess());

        Assert.IsFalse(logic.IsValidRating(0));
        Assert.IsFalse(logic.IsValidRating(6));
    }

    [TestMethod]
    public void IsValidComment_ReturnsFalseForEmptyOrTooShortText()
    {
        var logic = new ReviewLogic(new FakeReviewAccess());

        Assert.IsFalse(logic.IsValidComment(null));
        Assert.IsFalse(logic.IsValidComment("  "));
        Assert.IsFalse(logic.IsValidComment("ok"));
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
            Rating = 4,
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
            Rating = 6,
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
            Rating = 4,
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
            Rating = 5,
            Comment = "  Nice story and music  "
        };

        logic.SaveReview(review);

        Assert.AreEqual(1, fakeAccess.UpsertCalls);
        Assert.IsNotNull(fakeAccess.LastSavedReview);
        Assert.AreEqual("Nice story and music", fakeAccess.LastSavedReview!.Comment);
        Assert.AreEqual(9, fakeAccess.LastSavedReview.GameId);
        Assert.AreEqual(4, fakeAccess.LastSavedReview.CustomerId);
        Assert.AreEqual(5, fakeAccess.LastSavedReview.Rating);
    }

    [TestMethod]
    public void DeleteReview_Throws_WhenReviewIsNull()
    {
        var logic = new ReviewLogic(new FakeReviewAccess());

        Assert.ThrowsException<ArgumentNullException>(() => logic.DeleteReview(null!));
    }

    [TestMethod]
    public void DeleteReview_CallsAccessDelete()
    {
        var fakeAccess = new FakeReviewAccess();
        var logic = new ReviewLogic(fakeAccess);
        var review = new ReviewModel { Id = 7, GameId = 2 };

        logic.DeleteReview(review);

        Assert.AreEqual(1, fakeAccess.DeleteCalls);
    }

    [TestMethod]
    public void ToggleReviewVisibility_UpsertsWithToggledHiddenValue()
    {
        var fakeAccess = new FakeReviewAccess
        {
            ReviewToReturn = new ReviewModel { Id = 5, GameId = 1, IsHidden = false }
        };
        var logic = new ReviewLogic(fakeAccess);

        logic.ToggleReviewVisibility(5);

        Assert.AreEqual(1, fakeAccess.UpsertCalls);
        Assert.IsNotNull(fakeAccess.LastSavedReview);
        Assert.IsTrue(fakeAccess.LastSavedReview!.IsHidden);
    }

    [TestMethod]
    public void GetAllReviewsForGameAdmin_ReturnsEmpty_WhenGameIdIsInvalid()
    {
        var logic = new ReviewLogic(new FakeReviewAccess());

        var result = logic.GetAllReviewsForGameAdmin(0);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
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
