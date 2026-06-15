using Npgsql;
using Dapper;

public class PermissionsAccess
{
    public bool HasPermission(int accountId, string permissionName)
    {
        using var connection = new NpgsqlConnection(AppConfig.PostgresConnectionString);
        
        // account roles -> role permissions -> permissions
        string sql = @"
            SELECT COUNT(1)
            FROM account_roles ar
            JOIN role_permissions rp ON ar.role_id = rp.role_id
            JOIN permissions p ON rp.permission_id = p.id
            WHERE ar.account_id = @AccountId AND p.name = @PermissionName";

        // If count > 0 the user has the permission
        int count = connection.ExecuteScalar<int>(sql, new { AccountId = accountId, PermissionName = permissionName });
        
        return count > 0;
    }
}