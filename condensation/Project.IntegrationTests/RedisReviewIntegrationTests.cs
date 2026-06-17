using Xunit;

public class RedisReviewIntegrationTests
{
    private const string RedisConn = "pg-hro.crazyelectron.io:6300,password=phai8ohV2giet7pu1de5bei5ahw9oh"; // i cry because i want to work in cybersecurity

    private const int TestGameId = 999999;
    private const int TestCustomerId = 999999;

    public RedisReviewIntegrationTests()
    {
        AppConfig.RedisConnectionString = RedisConn;
    }

    [Fact]
    public void UpsertReview_ActuallyLandsInRedis_AndCanBeReadBack()
    {
        var access = new ReviewAccess(); // Redis-implementation

        var review = new ReviewModel
        {
            GameId = TestGameId,
            CustomerId = TestCustomerId,
            Rating = 5,
            Title = "Integrationtest",
            Comment = "Store in Redis please!"
        };

        try
        {
            access.UpsertReview(review);

            Assert.True(review.Id > 0, "Saving failed due to missing ID");

            var fromRedis = access.GetReviewById(review.Id);
            Assert.NotNull(fromRedis);
            Assert.Equal(review.Comment, fromRedis!.Comment);
            Assert.Equal(5, fromRedis.Rating);
            Assert.Equal(TestGameId, fromRedis.GameId);

            var all = access.GetAllReviewsForGameAdmin(TestGameId);
            Assert.Contains(all, r => r.Id == review.Id);
        }
        finally
        {
            if (review.Id > 0)
                access.DeleteReview(review.Id, TestGameId);
        }
    }

    [Fact]
    public void DeleteReview_RemovesItFromRedis()
    {
        var access = new ReviewAccess();

        var review = new ReviewModel
        {
            GameId = TestGameId,
            CustomerId = TestCustomerId,
            Rating = 3,
            Comment = "temporary review"
        };

        access.UpsertReview(review);
        Assert.True(review.Id > 0);

        access.DeleteReview(review.Id, TestGameId);

        Assert.Null(access.GetReviewById(review.Id));
    }
}