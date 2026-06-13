public class PermissionsLogic
{
    private readonly PermissionsAccess _permissionsAccess = new PermissionsAccess();

    public bool HasPermission(int accountId, string permissionName)
    {
        // Guests (not logged in) have an ID of 0 or -1
        if (accountId <= 0) return false;

        return _permissionsAccess.HasPermission(accountId, permissionName);
    }

    public HashSet<string> GetPermissions(int accountId)
    {
        if (accountId <= 0) return new HashSet<string>();

        return _permissionsAccess.GetPermissionsForAccount(accountId);
    }
}