public static class UserActionLogger
{
    private static readonly UserActionLogAccess _access = new();

    public static void Log(string actionType, string objectType, string? objectId = null, Dictionary<string, object?>? details = null)
    {
        try
        {
            var user = CurrentUserModel.CurrentUser;

            if (user == null)
            {
                return;
            }

            var userAction = new UserActionLogModel
            {
                ActionType = actionType,
                CreatedAtUtc = DateTime.UtcNow,
                UserId = user.Id,
                ObjectType = objectType,
                ObjectId = objectId,
                Details = details ?? new Dictionary<string, object?>()
            };

            _access.Insert(userAction);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not save user action:");
            Console.WriteLine(ex.Message);
        }
    }

    public static void LogError(string message, string source, Exception? exception = null, Dictionary<string, object?>? details = null)
    {
        if (details == null)
        {
            details = new Dictionary<string, object?>();
        }

        details["message"] = message;
        details["source"] = source;

        if (exception != null)
        {
            details["errorType"] = exception.GetType().Name;
            details["errorMessage"] = exception.Message;
        }

        Log(
            actionType: "error",
            objectType: "system",
            objectId: source,
            details: details
        );
    }
}