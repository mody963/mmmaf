// using MongoDB.Bson;

// public static class UserActionLogger
// {
//     private static readonly UserActionLogAccess _access = new();

//     public static void Log(
//         string actionType,
//         string objectType,
//         string? objectId = null,
//         BsonDocument? details = null)
//     {
//         try
//         {
//             var user = CurrentUserModel.CurrentUser;

//             if (user == null)
//             {
//                 return;
//             }

//             if (details == null)
//             {
//                 details = new BsonDocument();
//             }

//             var userAction = new UserActionLogModel
//             {
//                 ActionType = actionType,
//                 CreatedAtUtc = DateTime.UtcNow,
//                 UserId = user.Id,
//                 ObjectType = objectType,
//                 ObjectId = objectId,
//                 Details = details
//             };

//             _access.Insert(userAction);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine("Could not save user action:");
//             Console.WriteLine(ex.ToString());
//         }
//     }

//     public static void LogError(
//         string message,
//         string source,
//         Exception? exception = null,
//         BsonDocument? details = null)
//     {
//         if (details == null)
//         {
//             details = new BsonDocument();
//         }

//         details["message"] = message;
//         details["source"] = source;

//         if (exception != null)
//         {
//             details["errorType"] = exception.GetType().Name;
//             details["errorMessage"] = exception.Message;
//         }

//         Log(
//             actionType: "error",
//             objectType: "system",
//             objectId: source,
//             details: details
//         );
//     }
// }

using MongoDB.Bson;

public static class UserActionLogger
{
    private static UserActionLogAccess? _access;

    public static void Log(
        string actionType,
        string objectType,
        string? objectId = null,
        BsonDocument? details = null)
    {
        try
        {
            Console.WriteLine($"DEBUG LOGGER: trying to log {actionType}");

            var user = CurrentUserModel.CurrentUser;

            if (user == null)
            {
                Console.WriteLine("DEBUG LOGGER: skipped, current user is null");
                return;
            }

            if (details == null)
            {
                details = new BsonDocument();
            }

            var userAction = new UserActionLogModel
            {
                ActionType = actionType,
                CreatedAtUtc = DateTime.UtcNow,
                UserId = user.Id,
                ObjectType = objectType,
                ObjectId = objectId,
                Details = details
            };

            if (_access == null)
            {
                Console.WriteLine("DEBUG LOGGER: creating access");
                _access = new UserActionLogAccess();
            }

            _access.Insert(userAction);

            Console.WriteLine($"DEBUG LOGGER: saved {actionType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Could not save user action:");
            Console.WriteLine(ex.ToString());
        }
    }

    public static void LogError(
        string message,
        string source,
        Exception? exception = null,
        BsonDocument? details = null)
    {
        if (details == null)
        {
            details = new BsonDocument();
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