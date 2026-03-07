public static class DbPath
{
    public static string Get()
    {
        // When running, this is ...\bin\Debug\netX.Y\
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        // Go back to the project root (..\..\.. from bin folder)
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));

        // Ensure DataSources exists
        string dataDir = Path.Combine(projectRoot, "DataSources");
        Directory.CreateDirectory(dataDir);

        // Return absolute db path
        return Path.Combine(dataDir, "CinemajesticDataBases.db");
    }
}
