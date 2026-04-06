using Npgsql;

namespace CondensationApp;

public class Database
{
    private readonly string _connectionString;

    public Database(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task TestConnectionAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            Console.WriteLine("Databaseverbinding gelukt.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Fout bij verbinden:");
            Console.WriteLine(ex.ToString());
        }
    }

    public async Task EnsureAnalyticsViewsAsync()
    {
        string sql = @"
    CREATE OR REPLACE VIEW vw_user_revenue_last_month AS
    SELECT
        DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')::date AS period_start,
        (DATE_TRUNC('month', CURRENT_DATE) - INTERVAL '1 day')::date AS period_end,
        COALESCE(SUM(o.total_price), 0)::numeric(12,2) AS total_revenue
    FROM orders o
    WHERE o.order_date >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
      AND o.order_date < DATE_TRUNC('month', CURRENT_DATE);

    CREATE OR REPLACE VIEW vw_user_revenue_last_year AS
    SELECT
        DATE_TRUNC('year', CURRENT_DATE - INTERVAL '1 year')::date AS period_start,
        (DATE_TRUNC('year', CURRENT_DATE) - INTERVAL '1 day')::date AS period_end,
        COALESCE(SUM(o.total_price), 0)::numeric(12,2) AS total_revenue
    FROM orders o
    WHERE o.order_date >= DATE_TRUNC('year', CURRENT_DATE - INTERVAL '1 year')
      AND o.order_date < DATE_TRUNC('year', CURRENT_DATE);

    CREATE OR REPLACE VIEW vw_user_top_3_most_expensive_games AS
    SELECT
        g.id,
        g.title AS game_name,
        g.price
    FROM game g
    WHERE g.is_active = true
    ORDER BY g.price DESC, g.title ASC
    LIMIT 3;

    CREATE OR REPLACE VIEW vw_user_top_3_cheapest_games AS
    SELECT
        g.id,
        g.title AS game_name,
        g.price
    FROM game g
    WHERE g.is_active = true
    ORDER BY g.price ASC, g.title ASC
    LIMIT 3;

    CREATE OR REPLACE VIEW vw_user_top_3_genres_most_sold AS
    SELECT
        ge.id,
        ge.name AS genre_name,
        SUM(og.quantity) AS sold_copies
    FROM order_games og
    JOIN game g ON g.id = og.game_id
    JOIN genre ge ON ge.id = g.genre_id
    GROUP BY ge.id, ge.name
    ORDER BY sold_copies DESC, ge.name ASC
    LIMIT 3;

    CREATE OR REPLACE VIEW vw_admin_top_10_games_last_month AS
    SELECT
        g.id,
        g.title AS game_name,
        SUM(og.quantity) AS sold_copies
    FROM orders o
    JOIN order_games og ON og.order_id = o.id
    JOIN game g ON g.id = og.game_id
    WHERE o.order_date >= DATE_TRUNC('month', CURRENT_DATE - INTERVAL '1 month')
      AND o.order_date < DATE_TRUNC('month', CURRENT_DATE)
    GROUP BY g.id, g.title
    ORDER BY sold_copies DESC, g.title ASC
    LIMIT 10;

    CREATE OR REPLACE VIEW vw_admin_top_10_genres AS
    SELECT
        ge.id,
        ge.name AS genre_name,
        SUM(og.quantity) AS sold_copies
    FROM order_games og
    JOIN game g ON g.id = og.game_id
    JOIN genre ge ON ge.id = g.genre_id
    GROUP BY ge.id, ge.name
    ORDER BY sold_copies DESC, ge.name ASC
    LIMIT 10;
    ";

    await using var conn = new NpgsqlConnection(_connectionString);
    await conn.OpenAsync();

    await using var cmd = new NpgsqlCommand(sql, conn);
    await cmd.ExecuteNonQueryAsync(); // ExecuteNonQueryAsync is used for commands that don't return results (like CREATE VIEW)
    }

    public async Task EnsureReviewSchemaAsync()
    {
        string sql = @"
    ALTER TABLE reviews
    ADD COLUMN IF NOT EXISTS comment TEXT NOT NULL DEFAULT '';

    ALTER TABLE reviews
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP;
    ";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }
}