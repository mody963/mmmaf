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

    private class FakeReviewAccess : IReviewAccess
    {
        public bool HasPurchased { get; set; }
        public int UpsertCalls { get; private set; }
        public ReviewModel? LastSavedReview { get; private set; }

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
    }
}
