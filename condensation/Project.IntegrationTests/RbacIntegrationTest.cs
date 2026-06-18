using Xunit;
using Npgsql;
using Dapper;

public class RbacPermissionsIntegrationTests : IDisposable
{
    private const string PostgresConn = "Host=localhost;Port=5432;Database=Condensation;Username=menno;Password=test12345";

    private readonly PermissionsAccess _access;
    private readonly string _testEmail;
    private readonly int _testAccountId;

    public RbacPermissionsIntegrationTests()
    {
        AppConfig.PostgresConnectionString = PostgresConn;
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        _access = new PermissionsAccess();

        // Unique email via Guid so the test never messes with existing data
        _testEmail = $"rbac_test_{Guid.NewGuid()}@test.local";

        using var conn = new NpgsqlConnection(PostgresConn);

        // Create a temp account for testing
        _testAccountId = conn.ExecuteScalar<int>(
            @"INSERT INTO account (email, first_name, last_name, password, role, is_active)
              VALUES (@Email, 'Rbac', 'Test', 'x', 1, true)
              RETURNING id;",
            new { Email = _testEmail });

        // Makee it admin
        conn.Execute(
            "INSERT INTO account_roles (account_id, role_id) VALUES (@AccountId, 1);",
            new { AccountId = _testAccountId });
    }

    [Fact]
    public void GetPermissionsForAccount_AdminAccount_ReturnsAdminPermissions()
    {
        var permissions = _access.GetPermissionsForAccount(_testAccountId);

        Assert.Contains("games.create", permissions);
        Assert.Contains("analytics.read", permissions);
        Assert.DoesNotContain("orders.create", permissions);
    }

    [Fact]
    public void HasPermission_UnknownAccount_ReturnsFalse()
    {
        // an account that has no roles linked has no permissions at all
        Assert.False(_access.HasPermission(-1, "games.create"));
    }

    // cleans up the test account and roles after the tests run
    public void Dispose()
    {
        using var conn = new NpgsqlConnection(PostgresConn);
        conn.Execute(
            @"DELETE FROM account_roles WHERE account_id = @AccountId;
              DELETE FROM account WHERE id = @AccountId;",
            new { AccountId = _testAccountId });
    }
}