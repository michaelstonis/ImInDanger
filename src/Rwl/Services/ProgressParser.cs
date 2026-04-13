using System.Text.RegularExpressions;
using Rwl.Models;

namespace Rwl.Services;

public static partial class ProgressParser
{
    public static List<ProgressEntry> Parse(string filePath)
    {
        if (!File.Exists(filePath))
            return [];

        var content = File.ReadAllText(filePath);
        var entries = new List<ProgressEntry>();

        // Split on ## Iteration headers
        var blocks = IterationSplit().Split(content);

        foreach (var block in blocks)
        {
            var header = IterationHeader().Match(block);
            if (!header.Success) continue;

            var iterNum = int.TryParse(header.Groups[1].Value, out var n) ? n : 0;
            var timestamp = header.Groups.Count > 2 ? header.Groups[2].Value.Trim() : null;

            string? task = null, status = null, validation = null, notes = null;
            var changes = new List<string>();

            foreach (var line in block.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("**Task:**", StringComparison.Ordinal))
                    task = trimmed["**Task:**".Length..].Trim();
                else if (trimmed.StartsWith("**Status:**", StringComparison.Ordinal))
                    status = trimmed["**Status:**".Length..].Trim();
                else if (trimmed.StartsWith("**Validation:**", StringComparison.Ordinal))
                    validation = trimmed["**Validation:**".Length..].Trim();
                else if (trimmed.StartsWith("**Notes:**", StringComparison.Ordinal))
                    notes = trimmed["**Notes:**".Length..].Trim();
                else if (trimmed.StartsWith("- ", StringComparison.Ordinal) && changes.Count < 20)
                    changes.Add(trimmed[2..]);
            }

            entries.Add(new ProgressEntry
            {
                Iteration = iterNum,
                Timestamp = timestamp,
                Task = task,
                Status = status,
                Changes = changes,
                Validation = validation,
                Notes = notes,
                RawBlock = block.Trim(),
            });
        }

        return entries;
    }

    public static (int Total, int Successes, int Failures, int Lines) Stats(string filePath)
    {
        if (!File.Exists(filePath))
            return (0, 0, 0, 0);

        var content = File.ReadAllText(filePath);
        var lines = File.ReadAllLines(filePath).Length;
        var total = IterationCountPattern().Matches(content).Count;
        var successes = SuccessPattern().Matches(content).Count;
        var failures = FailurePattern().Matches(content).Count;
        return (total, successes, failures, lines);
    }

    [GeneratedRegex(@"(?=^## Iteration)", RegexOptions.Multiline)]
    private static partial Regex IterationSplit();

    [GeneratedRegex(@"^## Iteration\s+(\d+)\s*(?:—\s*(.+))?$", RegexOptions.Multiline)]
    private static partial Regex IterationHeader();

    [GeneratedRegex(@"^## Iteration", RegexOptions.Multiline)]
    private static partial Regex IterationCountPattern();

    [GeneratedRegex(@"✅ Success")]
    private static partial Regex SuccessPattern();

    [GeneratedRegex(@"❌ Failed")]
    private static partial Regex FailurePattern();
}
