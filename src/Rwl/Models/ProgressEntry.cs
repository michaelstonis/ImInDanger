namespace Rwl.Models;

public sealed class ProgressEntry
{
    public int Iteration { get; init; }
    public string? Timestamp { get; init; }
    public string? Task { get; init; }
    public string? Status { get; init; }
    public List<string> Changes { get; init; } = [];
    public string? Validation { get; init; }
    public string? Notes { get; init; }
    public string RawBlock { get; init; } = "";

    public bool IsSuccess => Status?.Contains("Success", StringComparison.OrdinalIgnoreCase) == true
                          || Status?.Contains("✅", StringComparison.Ordinal) == true;

    public bool IsFailure => Status?.Contains("Failed", StringComparison.OrdinalIgnoreCase) == true
                          || Status?.Contains("❌", StringComparison.Ordinal) == true;
}
