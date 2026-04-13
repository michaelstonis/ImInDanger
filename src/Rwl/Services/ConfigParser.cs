using System.Text.RegularExpressions;
using Rwl.Models;

namespace Rwl.Services;

public static partial class ConfigParser
{
    public static LoopConfig Parse(string filePath)
    {
        if (!File.Exists(filePath))
            return new LoopConfig();

        var content = File.ReadAllText(filePath);
        var lines = File.ReadAllLines(filePath);

        var maxIter = ExtractInt(content, MaxIterPattern()) ?? 20;
        var timeout = ExtractInt(content, TimeoutPattern()) ?? 10;
        var autoReview = ExtractInt(content, AutoReviewPattern()) ?? 5;
        var maxLines = ExtractInt(content, MaxLinesPattern()) ?? 200;
        var maxFiles = ExtractInt(content, MaxFilesPattern()) ?? 10;

        var validationCmds = ExtractListSection(lines, "Validation Commands");
        var allowedPaths = ExtractListSection(lines, "Allowed Paths");
        var restrictedPaths = ExtractListSection(lines, "Restricted Paths");

        return new LoopConfig
        {
            MaxIterations = maxIter,
            TimeoutMinutes = timeout,
            AutoReviewInterval = autoReview,
            MaxLinesPerIteration = maxLines,
            MaxFilesPerIteration = maxFiles,
            ValidationCommands = validationCmds,
            AllowedPaths = allowedPaths,
            RestrictedPaths = restrictedPaths,
        };
    }

    private static int? ExtractInt(string content, Regex pattern)
    {
        var match = pattern.Match(content);
        return match.Success && int.TryParse(match.Groups[1].Value, out var val) ? val : null;
    }

    private static List<string> ExtractListSection(string[] lines, string sectionName)
    {
        var results = new List<string>();
        var inSection = false;

        foreach (var line in lines)
        {
            if (line.Contains(sectionName, StringComparison.OrdinalIgnoreCase) && line.TrimStart().StartsWith('#'))
            {
                inSection = true;
                continue;
            }

            if (inSection)
            {
                if (line.TrimStart().StartsWith('#'))
                    break;
                var trimmed = line.Trim();
                if (trimmed.StartsWith("- ", StringComparison.Ordinal) || trimmed.StartsWith("* ", StringComparison.Ordinal))
                {
                    var val = trimmed[2..].Trim().Trim('`');
                    if (!string.IsNullOrEmpty(val))
                        results.Add(val);
                }
            }
        }

        return results;
    }

    [GeneratedRegex(@"max_iterations:\s*(\d+)")]
    private static partial Regex MaxIterPattern();

    [GeneratedRegex(@"timeout_minutes:\s*(\d+)")]
    private static partial Regex TimeoutPattern();

    [GeneratedRegex(@"auto_review_interval:\s*(\d+)")]
    private static partial Regex AutoReviewPattern();

    [GeneratedRegex(@"max_lines_per_iteration:\s*(\d+)")]
    private static partial Regex MaxLinesPattern();

    [GeneratedRegex(@"max_files_per_iteration:\s*(\d+)")]
    private static partial Regex MaxFilesPattern();
}
