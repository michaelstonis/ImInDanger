using System.Reflection;

namespace Rwl.Services;

public static class ResourceLoader
{
    private static readonly Assembly Assembly = typeof(ResourceLoader).Assembly;

    public static string? LoadTemplate(string name)
    {
        // Embedded resources use dots as path separators
        var resourceName = $"Rwl.Resources.Templates.{name.Replace('/', '.')}";
        using var stream = Assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static IEnumerable<string> ListTemplates()
    {
        var prefix = "Rwl.Resources.Templates.";
        return Assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix, StringComparison.Ordinal))
            .Select(n => n[prefix.Length..]);
    }
}
