using System.Reflection;
using System.Resources;

// This class is responsible for fetching the correct text based on the current culture.
public static class Texts
{
    private static readonly ResourceManager _resourceManager =
        new ResourceManager("Project.Resources.Resources",
            Assembly.GetExecutingAssembly());

    public static string Get(string key)
    {
        return _resourceManager.GetString(key)
               ?? $"Missing: {key}";
    }
}