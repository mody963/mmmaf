public static class SessionManager
{
    public static readonly string SessionId = GenerateSessionId();

    private static string GenerateSessionId()
    {
        string datePart = DateTime.Now.ToString("yyyyMMdd");
        string randomPart = new Random().Next(1000, 9999).ToString(); 
        
        return $"SESSION-{datePart}-{randomPart}";
    }
}