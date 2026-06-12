public class PermissionsLogic
{
    private readonly PermissionsAccess _permissionsAccess = new PermissionsAccess();

    public bool HasPermission(int accountId, string permissionName)
    {
        // Guests (not logged in) usually have an ID of 0 or -1
        if (accountId <= 0)
        {
            return false; 
        }

        return _permissionsAccess.HasPermission(accountId, permissionName);
    }
}