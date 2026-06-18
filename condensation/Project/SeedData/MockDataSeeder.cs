using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using StackExchange.Redis;

public static class MockDataSeeder
{
    private const int DefaultAccountCount = 5000;
    private static readonly string[] FirstNames = new[]
    {
        "Luca", "Mila", "Noah", "Sophie", "Emma", "Liam", "Ava", "Finn", "Ella", "Lucas",
        "Nora", "Jules", "Mats", "Sanne", "Tess", "Sem", "Lisa", "Jade", "Daan", "Levi"
    };

    private static readonly string[] LastNames = new[]
    {
        "Bakker", "Jansen", "de Vries", "Smits", "Visser", "Smit", "Meijer", "Dekker", "Brouwer", "Bos",
        "Mulder", "Kramer", "Willems", "Schouten", "Hendriks", "de Boer", "Peters", "van Dijk", "Vos", "Van Dam"
    };

    private static readonly string[] Genres = new[]
    {
        "Action", "Adventure", "Strategy", "RPG", "Simulation", "Sports", "Puzzle", "Horror", "Platformer", "Racing"
    };

    private static readonly string[] AgeRatings = new[]
    {
        "E", "T", "M", "A"
    };

    private static readonly string[] PaymentMethods = new[]
    {
        "Visa", "Mastercard", "iDeal", "Paypal", "Creditcard"
    };

    private static readonly string[] Addresses = new[]
    {
        "Dorpsstraat 12, Amsterdam", "Kerklaan 7, Utrecht", "Stationsplein 3, Rotterdam",
        "Markt 21, Den Haag", "Lindelaan 5, Eindhoven", "Zandstraat 9, Groningen", "Hoofdstraat 42, Breda",
        "Prinses Margrietstraat 88, Nijmegen", "Laan van Meerdervoort 16, Leiden", "Vismarkt 14, Maastricht"
    };

    private static readonly string[] StudioPrefixes = new[]
    {
        "Nova", "Pixel", "Steel", "Thunder", "Silver", "Shadow", "Cosmic", "Firefly", "Midnight", "Crystal"
    };

    private static readonly string[] StudioSuffixes = new[]
    {
        "Studios", "Games", "Interactive", "Works", "Labs", "Forge", "Entertainment", "Creations"
    };

    private static readonly string[] ReviewTitles = new[]
    {
        "Top game", "Geweldige ervaring", "Minder goed", "Aanrader", "Geen aanrader", "Verrassend leuk", "Mooi vormgegeven",
        "Verslavend", "Te eenvoudig", "Fantastisch verhaal"
    };

    private static readonly string[] ReviewPros = new[]
    {
        "Goede graphics", "Snelle gameplay", "Leuke characters", "Originele levels", "Goede besturing", "Mooie muziek"
    };

    private static readonly string[] ReviewCons = new[]
    {
        "Te kort", "Soms bugs", "Moeilijke moeilijkheid", "Weinig variatie", "Lange laadtijden", "Weinig uitdaging"
    };

    private static readonly string[] OrderStatuses = new[]
    {
        "Created", "Processing", "Shipped", "Delivered"
    };

    private static readonly string[] PaymentStatuses = new[]
    {
        "Pending", "Paid", "Failed"
    };

    public static async Task SeedAsync(int accountCount = DefaultAccountCount)
    {
        if (accountCount < 1)
            throw new ArgumentOutOfRangeException(nameof(accountCount));

        Console.WriteLine($"Start mockdata seed voor {accountCount} accounts...");

        await using var postgres = new NpgsqlConnection(AppConfig.PostgresConnectionString);
        await postgres.OpenAsync();

        var redis = ConnectionMultiplexer.Connect(AppConfig.RedisConnectionString);
        var redisDb = redis.GetDatabase();

        var mongoDb = AppConfig.MongoDb ?? new MongoDb(AppConfig.MongoDbConnectionString, AppConfig.MongoDbDatabaseName);
        await mongoDb.TestConnectionAsync();
        var orderDocuments = mongoDb.GetCollection<OrderDocumentModel>("orders");
        var previousOrders = mongoDb.GetCollection<BsonDocument>("previous_orders");

        await SeedLookupTablesAsync(postgres);

        var random = new Random(12345);
        var customerAccountIds = new List<int>();
        var publisherAccounts = new List<int>();

        int totalCustomers = (int)Math.Round(accountCount * 0.80);
        int totalPublishers = (int)Math.Round(accountCount * 0.15);
        int totalAdmins = accountCount - totalCustomers - totalPublishers;

        if (totalAdmins < 1)
        {
            totalAdmins = 1;
            totalCustomers = accountCount - totalPublishers - totalAdmins;
        }

        Console.WriteLine($"Seeds: {totalCustomers} customers, {totalPublishers} publishers, {totalAdmins} admins.");

        var accountCounter = 1;
        for (int i = 0; i < totalCustomers; i++)
        {
            int accountId = await CreateAccountAsync(postgres, accountCounter++, "customer", random);
            await CreateCustomerAsync(postgres, accountId, random);
            customerAccountIds.Add(accountId);
        }

        for (int i = 0; i < totalPublishers; i++)
        {
            int accountId = await CreateAccountAsync(postgres, accountCounter++, "publisher", random);
            await CreatePublisherAsync(postgres, accountId, random);
            publisherAccounts.Add(accountId);
        }

        for (int i = 0; i < totalAdmins; i++)
        {
            await CreateAccountAsync(postgres, accountCounter++, "admin", random);
        }

        Console.WriteLine($"Aangemaakt accounts en bijbehorende customer/publisher gegevens.");

        var publishers = await GetPublisherIdsAsync(postgres);
        var genreIds = await GetLookupIdsAsync(postgres, "genre");
        var ageRatingIds = await GetLookupIdsAsync(postgres, "age_rating");
        var gameIds = new List<int>();

        int gamesPerPublisher = 3;
        for (int i = 0; i < publishers.Count; i++)
        {
            for (int j = 0; j < gamesPerPublisher; j++)
            {
                int gameId = await CreateGameAsync(postgres, publishers[i], genreIds[random.Next(genreIds.Count)], ageRatingIds[random.Next(ageRatingIds.Count)], random);
                gameIds.Add(gameId);
            }

            await UpdatePublisherGameCountAsync(postgres, publishers[i], gamesPerPublisher);
        }

        Console.WriteLine($"Aangemaakt {gameIds.Count} games voor publishers.");

        int orderCount = Math.Max(500, accountCount / 5);
        var orderCustomerIds = await GetCustomerIdsAsync(postgres);
        for (int i = 1; i <= orderCount; i++)
        {
            int customerId = orderCustomerIds[random.Next(orderCustomerIds.Count)];
            int itemCount = random.Next(1, 4);
            var orderItems = new List<(int gameId, double price, int quantity)>();

            for (int itemIndex = 0; itemIndex < itemCount; itemIndex++)
            {
                int gameId = gameIds[random.Next(gameIds.Count)];
                double price = Math.Round(random.NextDouble() * 45 + 4.99, 2);
                int quantity = random.Next(1, 4);
                orderItems.Add((gameId, price, quantity));
            }

            int orderId = await CreateOrderAsync(postgres, customerId, orderItems, random);
            await CreateOrderDocumentAsync(orderDocuments, orderId, customerId, orderItems, random);
            await CreatePreviousOrderAsync(previousOrders, orderId, customerId, orderItems, random);
        }

        Console.WriteLine($"Aangemaakt {orderCount} orders in Postgres en MongoDB.");

        int reviewCount = Math.Max(500, accountCount / 10);
        for (int i = 1; i <= reviewCount; i++)
        {
            int customerId = orderCustomerIds[random.Next(orderCustomerIds.Count)];
            int gameId = gameIds[random.Next(gameIds.Count)];
            await CreateReviewRedisAsync(redisDb, customerId, gameId, random);
        }

        Console.WriteLine($"Aangemaakt {reviewCount} reviews in Redis.");
        Console.WriteLine("Mockdata seed voltooid.");
    }

    private static async Task SeedLookupTablesAsync(NpgsqlConnection conn)
    {
        foreach (var genre in Genres)
        {
            await using var genreCmd = new NpgsqlCommand(@"INSERT INTO genre (name)
                SELECT @Name
                WHERE NOT EXISTS (SELECT 1 FROM genre WHERE name = @Name);", conn);
            genreCmd.Parameters.AddWithValue("Name", genre);
            await genreCmd.ExecuteNonQueryAsync();
        }

        foreach (var rating in AgeRatings)
        {
            await using var ratingCmd = new NpgsqlCommand(@"INSERT INTO age_rating (name)
                SELECT @Name
                WHERE NOT EXISTS (SELECT 1 FROM age_rating WHERE name = @Name);", conn);
            ratingCmd.Parameters.AddWithValue("Name", rating);
            await ratingCmd.ExecuteNonQueryAsync();
        }
    }

    private static async Task<int> CreateAccountAsync(NpgsqlConnection conn, int sequence, string roleType, Random random)
    {
        var firstName = FirstNames[random.Next(FirstNames.Length)];
        var lastName = LastNames[random.Next(LastNames.Length)];
        string email = GenerateEmail(firstName, lastName, sequence);
        int role = roleType switch
        {
            "admin" => 1,
            "publisher" => 2,
            _ => 0
        };

        await using var cmd = new NpgsqlCommand(@"INSERT INTO account (email, first_name, last_name, password, role, is_active)
            VALUES (@Email, @FirstName, @LastName, @Password, @Role, true)
            RETURNING id;", conn);

        cmd.Parameters.AddWithValue("Email", email);
        cmd.Parameters.AddWithValue("FirstName", firstName);
        cmd.Parameters.AddWithValue("LastName", lastName);
        cmd.Parameters.AddWithValue("Password", "Password123!");
        cmd.Parameters.AddWithValue("Role", role);

        object? result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result!);
    }

    private static async Task CreateCustomerAsync(NpgsqlConnection conn, int accountId, Random random)
    {
        await using var cmd = new NpgsqlCommand(@"INSERT INTO customer (account_id, payment_method, address)
            VALUES (@AccountId, @PaymentMethod, @Address);", conn);

        cmd.Parameters.AddWithValue("AccountId", accountId);
        cmd.Parameters.AddWithValue("PaymentMethod", PaymentMethods[random.Next(PaymentMethods.Length)]);
        cmd.Parameters.AddWithValue("Address", Addresses[random.Next(Addresses.Length)]);

        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task CreatePublisherAsync(NpgsqlConnection conn, int accountId, Random random)
    {
        string studioName = $"{StudioPrefixes[random.Next(StudioPrefixes.Length)]} {StudioSuffixes[random.Next(StudioSuffixes.Length)]} {random.Next(100, 999)}";

        await using var cmd = new NpgsqlCommand(@"INSERT INTO publisher (account_id, studio_name, amount_of_games)
            VALUES (@AccountId, @StudioName, 0);", conn);

        cmd.Parameters.AddWithValue("AccountId", accountId);
        cmd.Parameters.AddWithValue("StudioName", studioName);

        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<List<int>> GetPublisherIdsAsync(NpgsqlConnection conn)
    {
        var result = new List<int>();
        await using var cmd = new NpgsqlCommand("SELECT id FROM publisher ORDER BY id", conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(reader.GetInt32(0));
        }

        return result;
    }

    private static async Task<List<int>> GetCustomerIdsAsync(NpgsqlConnection conn)
    {
        var result = new List<int>();
        await using var cmd = new NpgsqlCommand("SELECT id FROM customer ORDER BY id", conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(reader.GetInt32(0));
        }

        return result;
    }

    private static async Task<List<int>> GetLookupIdsAsync(NpgsqlConnection conn, string tableName)
    {
        var result = new List<int>();
        await using var cmd = new NpgsqlCommand($"SELECT id FROM {tableName} ORDER BY id", conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(reader.GetInt32(0));
        }

        return result;
    }

    private static async Task<int> CreateGameAsync(NpgsqlConnection conn, int publisherId, int genreId, int ageRatingId, Random random)
    {
        var title = GenerateGameTitle(random);
        var description = GenerateGameDescription(random);
        double price = Math.Round(random.NextDouble() * 45 + 4.99, 2);
        bool isActive = true;

        await using var cmd = new NpgsqlCommand(@"INSERT INTO game (publisher_id, title, description, genre_id, age_rating_id, price, is_active)
            VALUES (@PublisherId, @Title, @Description, @GenreId, @AgeRatingId, @Price, @IsActive)
            RETURNING id;", conn);

        cmd.Parameters.AddWithValue("PublisherId", publisherId);
        cmd.Parameters.AddWithValue("Title", title);
        cmd.Parameters.AddWithValue("Description", description);
        cmd.Parameters.AddWithValue("GenreId", genreId);
        cmd.Parameters.AddWithValue("AgeRatingId", ageRatingId);
        cmd.Parameters.AddWithValue("Price", price);
        cmd.Parameters.AddWithValue("IsActive", isActive);

        object? result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result!);
    }

    private static async Task UpdatePublisherGameCountAsync(NpgsqlConnection conn, int publisherId, int amount)
    {
        await using var cmd = new NpgsqlCommand(@"UPDATE publisher SET amount_of_games = amount_of_games + @Amount WHERE id = @PublisherId;", conn);
        cmd.Parameters.AddWithValue("Amount", amount);
        cmd.Parameters.AddWithValue("PublisherId", publisherId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<int> CreateOrderAsync(NpgsqlConnection conn, int customerId, List<(int gameId, double price, int quantity)> items, Random random)
    {
        decimal totalPrice = items.Sum(x => (decimal)x.price * x.quantity);
        DateTime orderDate = DateTime.UtcNow.AddDays(-random.Next(0, 180));

        await using var orderCmd = new NpgsqlCommand(@"INSERT INTO orders (customer_id, order_date, total_price)
            VALUES (@CustomerId, @OrderDate, @TotalPrice)
            RETURNING id;", conn);
        orderCmd.Parameters.AddWithValue("CustomerId", customerId);
        orderCmd.Parameters.AddWithValue("OrderDate", orderDate);
        orderCmd.Parameters.AddWithValue("TotalPrice", totalPrice);

        object? orderResult = await orderCmd.ExecuteScalarAsync();
        int orderId = Convert.ToInt32(orderResult!);

        foreach (var item in items)
        {
            await using var itemCmd = new NpgsqlCommand(@"INSERT INTO order_games (game_id, order_id, quantity)
                VALUES (@GameId, @OrderId, @Quantity);", conn);
            itemCmd.Parameters.AddWithValue("GameId", item.gameId);
            itemCmd.Parameters.AddWithValue("OrderId", orderId);
            itemCmd.Parameters.AddWithValue("Quantity", item.quantity);
            await itemCmd.ExecuteNonQueryAsync();
        }

        return orderId;
    }

    private static async Task CreateOrderDocumentAsync(IMongoCollection<OrderDocumentModel> collection, int orderId, int customerId, List<(int gameId, double price, int quantity)> items, Random random)
    {
        var document = new OrderDocumentModel
        {
            OrderNumber = $"ORD-{orderId:D6}",
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow.AddDays(-random.Next(0, 180)),
            ShippingAddress = Addresses[random.Next(Addresses.Length)],
            PaymentStatus = PaymentStatuses[random.Next(PaymentStatuses.Length)],
            OrderStatus = OrderStatuses[random.Next(OrderStatuses.Length)],
            TotalPrice = items.Sum(x => x.price * x.quantity)
        };

        foreach (var item in items)
        {
            document.Items.Add(new OrderItemModel(item.gameId, GenerateGameName(item.gameId), item.price, item.quantity));
        }

        document.StatusHistory.Add(new OrderStatusHistoryModel(document.OrderStatus, document.OrderDate));

        await collection.InsertOneAsync(document);
    }

    private static async Task CreatePreviousOrderAsync(IMongoCollection<BsonDocument> collection, int orderId, int customerId, List<(int gameId, double price, int quantity)> items, Random random)
    {
        var games = items.Select(item => new BsonDocument
        {
            { "gameId", item.gameId },
            { "gameTitle", GenerateGameName(item.gameId) },
            { "priceAtPurchase", item.price },
            { "quantity", item.quantity }
        }).ToList();

        var doc = new BsonDocument
        {
            { "id", orderId },
            { "customerId", customerId },
            { "orderDate", DateTime.UtcNow.AddDays(-random.Next(0, 180)) },
            { "totalPrice", items.Sum(x => x.price * x.quantity) },
            { "games", new BsonArray(games) }
        };

        await collection.InsertOneAsync(doc);
    }

    private static async Task CreateReviewRedisAsync(IDatabase redisDb, int customerId, int gameId, Random random)
    {
        int reviewId = (int)redisDb.StringIncrement("review:id_counter");
        var review = new ReviewModel
        {
            Id = reviewId,
            GameId = gameId,
            CustomerId = customerId,
            Title = ReviewTitles[random.Next(ReviewTitles.Length)],
            Comment = GenerateReviewComment(random),
            Pros = ReviewPros[random.Next(ReviewPros.Length)],
            Cons = ReviewCons[random.Next(ReviewCons.Length)],
            Rating = random.Next(1, 6),
            CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 180)),
            ReviewerName = GenerateReviewerName(customerId),
            IsHidden = random.NextDouble() < 0.10
        };

        string json = JsonSerializer.Serialize(review);
        string reviewKey = $"review:{review.Id}";
        string gameKey = $"game:{review.GameId}:reviews";
        string gameSortedKey = $"game:{review.GameId}:reviews:sorted";

        await redisDb.StringSetAsync(reviewKey, json);
        await redisDb.SetAddAsync(gameKey, review.Id);
        await redisDb.SortedSetAddAsync(gameSortedKey, review.Id, review.CreatedAt.Ticks);
    }

    private static string GenerateEmail(string firstName, string lastName, int sequence)
    {
        var normalized = firstName.ToLowerInvariant().Replace(" ", "") + "." + lastName.ToLowerInvariant().Replace(" ", "");
        return $"{normalized}.{sequence}@mockdata.local";
    }

    private static string GenerateGameTitle(Random random)
    {
        var subjects = new[] { "Quest", "Legacy", "Odyssey", "Rift", "Battle", "Kingdom", "Arena", "Shadow", "Chronicles", "Expedition" };
        var adjectives = new[] { "Dark", "Lost", "Galactic", "Brave", "Mystic", "Frozen", "Neo", "Thunder", "Ancient", "Silent" };
        return $"{adjectives[random.Next(adjectives.Length)]} {subjects[random.Next(subjects.Length)]}";
    }

    private static string GenerateGameDescription(Random random)
    {
        var phrases = new[]
        {
            "A thrilling adventure with fast-paced combat.",
            "Discover hidden secrets in a sprawling fantasy world.",
            "Test your strategy skills against intelligent opponents.",
            "Build your perfect team and conquer every challenge.",
            "Experience emotional storytelling and cinematic moments.",
            "Explore a rich universe filled with dangerous mysteries.",
            "Enjoy beautiful visuals and immersive gameplay.",
            "Master deep mechanics and intense action sequences.",
            "Complete quests, upgrade equipment and save the galaxy.",
            "A creative and unpredictable journey through unique levels."
        };

        return phrases[random.Next(phrases.Length)];
    }

    private static string GenerateReviewComment(Random random)
    {
        var fragments = new[]
        {
            "Laat weinig te wensen over.", "Het kan beter, maar het is leuk.", "Ik speelde uren door.", "De controls voelen goed.", "Graphics zijn erg sterk.", "Muziek past perfect bij de sfeer.", "Soms is de moeilijkheid niet goed gebalanceerd.", "Het verhaal is boeiend.", "Er zitten goede verrassingen in.", "Helaas liep het een keer vast."
        };
        int numberOfFragments = random.Next(2, 4);
        return string.Join(" ", Enumerable.Range(0, numberOfFragments).Select(_ => fragments[random.Next(fragments.Length)]));
    }

    private static string GenerateReviewerName(int customerId)
    {
        return $"Reviewer{customerId}";
    }

    private static string GenerateGameName(int gameId)
    {
        return $"Game #{gameId}";
    }
}
