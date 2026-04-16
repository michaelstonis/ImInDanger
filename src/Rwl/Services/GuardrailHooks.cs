using GitHub.Copilot.SDK;
using Rwl.Models;

namespace Rwl.Services;

/// <summary>
/// Session hooks that enforce guardrails from LOOP_CONFIG.md.
/// Restricts file access, blocks dangerous patterns, and enforces limits.
/// </summary>
public static class GuardrailHooks
{
    // Tool names that involve file system writes
    private static readonly HashSet<string> FileWriteTools = new(StringComparer.OrdinalIgnoreCase)
    {
        "edit_file", "create_file", "write_file", "delete_file",
        "insert_edit_into_file", "replace_string_in_file",
    };

    private static readonly HashSet<string> DangerousPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "rm -rf",
        "git push --force",
        "git reset --hard",
    };

    public static SessionHooks Create(string workingDirectory)
    {
        var configPath = Path.Combine(workingDirectory, "LOOP_CONFIG.md");

        return new SessionHooks
        {
            OnPreToolUse = (input, _invocation) => HandlePreToolUse(input, configPath, workingDirectory),
        };
    }

    private static Task<PreToolUseHookOutput?> HandlePreToolUse(
        PreToolUseHookInput input,
        string configPath,
        string workingDirectory)
    {
        var config = ConfigParser.Parse(configPath);
        var toolName = input.ToolName ?? "";
        var toolArgs = input.ToolArgs?.ToString() ?? "";

        // Check for dangerous shell patterns in any tool args
        foreach (var pattern in DangerousPatterns)
        {
            if (toolArgs.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<PreToolUseHookOutput?>(new PreToolUseHookOutput
                {
                    PermissionDecision = "deny",
                    PermissionDecisionReason = $"Blocked: dangerous pattern '{pattern}' detected in tool arguments.",
                });
            }
        }

        // Enforce path restrictions for file-write tools
        if (FileWriteTools.Contains(toolName))
        {
            var pathCheck = CheckPathRestrictions(toolArgs, config, workingDirectory);
            if (pathCheck is not null)
                return Task.FromResult<PreToolUseHookOutput?>(pathCheck);
        }

        // Allow everything else
        return Task.FromResult<PreToolUseHookOutput?>(new PreToolUseHookOutput
        {
            PermissionDecision = "allow",
        });
    }

    private static PreToolUseHookOutput? CheckPathRestrictions(
        string toolArgs,
        LoopConfig config,
        string workingDirectory)
    {
        // Try to extract file path from tool arguments (common patterns)
        var filePath = ExtractFilePath(toolArgs);
        if (filePath is null)
            return null; // Can't determine path — allow but rely on other checks

        // Normalize to relative path
        if (Path.IsPathRooted(filePath))
        {
            filePath = Path.GetRelativePath(workingDirectory, filePath);
        }

        // Check restricted paths (deny list)
        foreach (var restricted in config.RestrictedPaths)
        {
            if (filePath.StartsWith(restricted, StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains($"/{restricted}", StringComparison.OrdinalIgnoreCase))
            {
                return new PreToolUseHookOutput
                {
                    PermissionDecision = "deny",
                    PermissionDecisionReason = $"Path '{filePath}' is in the restricted paths list ({restricted}).",
                };
            }
        }

        // Check allowed paths (allow list — only if configured)
        if (config.AllowedPaths.Count > 0)
        {
            var allowed = config.AllowedPaths.Any(ap =>
                filePath.StartsWith(ap, StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains($"/{ap}", StringComparison.OrdinalIgnoreCase));

            if (!allowed)
            {
                return new PreToolUseHookOutput
                {
                    PermissionDecision = "deny",
                    PermissionDecisionReason = $"Path '{filePath}' is not in the allowed paths list.",
                };
            }
        }

        return null;
    }

    private static string? ExtractFilePath(string toolArgs)
    {
        // The tool args are typically JSON — try to find a "path" or "file" field
        // This is a best-effort extraction without a JSON dependency
        foreach (var key in new[] { "\"path\"", "\"file\"", "\"filePath\"", "\"file_path\"" })
        {
            var idx = toolArgs.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) continue;

            var colonIdx = toolArgs.IndexOf(':', idx + key.Length);
            if (colonIdx < 0) continue;

            var quoteStart = toolArgs.IndexOf('"', colonIdx + 1);
            if (quoteStart < 0) continue;

            var quoteEnd = toolArgs.IndexOf('"', quoteStart + 1);
            if (quoteEnd < 0) continue;

            return toolArgs[(quoteStart + 1)..quoteEnd];
        }

        return null;
    }
}
