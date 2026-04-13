namespace Rwl.Services;

public static class RwlHome
{
    private static string? _home;

    public static string Resolve()
    {
        if (_home is not null)
            return _home;

        // 1. Check RWL_HOME env var
        var envHome = Environment.GetEnvironmentVariable("RWL_HOME");
        if (!string.IsNullOrEmpty(envHome) && Directory.Exists(envHome))
        {
            _home = envHome;
            return _home;
        }

        // 2. Check relative to binary location
        var binDir = AppContext.BaseDirectory;

        // Binary might be at <project>/bin/rwl or <project>/src/Rwl/bin/.../rwl
        var candidates = new[]
        {
            Path.GetFullPath(Path.Combine(binDir, "..")),        // bin/rwl → project root
            Path.GetFullPath(Path.Combine(binDir, "..", "..")),   // deeper bin location
            Path.GetFullPath(Path.Combine(binDir, "..", "..", "..", "..", "..")), // src/Rwl/bin/Debug/net*/rwl
        };

        foreach (var candidate in candidates)
        {
            if (File.Exists(Path.Combine(candidate, "templates", "TASKS.md")))
            {
                _home = candidate;
                return _home;
            }
        }

        // 3. Fallback: use ~/.rwl
        _home = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rwl");
        return _home;
    }
}
