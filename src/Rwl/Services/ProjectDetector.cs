using Rwl.Models;

namespace Rwl.Services;

public static class ProjectDetector
{
    public static ProjectInfo Detect(string directory = ".")
    {
        var type = "unknown";
        string? testCmd = null, buildCmd = null, lintCmd = null;
        var sourceDirs = new List<string>();

        if (File.Exists(Path.Combine(directory, "package.json")))
        {
            type = "node";
            testCmd = "npm test";
            buildCmd = "npm run build";
            lintCmd = "npm run lint";
        }
        else if (File.Exists(Path.Combine(directory, "go.mod")))
        {
            type = "go";
            testCmd = "go test ./...";
            buildCmd = "go build ./...";
            lintCmd = "golangci-lint run";
        }
        else if (File.Exists(Path.Combine(directory, "Cargo.toml")))
        {
            type = "rust";
            testCmd = "cargo test";
            buildCmd = "cargo build";
            lintCmd = "cargo clippy";
        }
        else if (Directory.GetFiles(directory, "*.csproj").Length > 0
              || Directory.GetFiles(directory, "*.sln").Length > 0)
        {
            type = "dotnet";
            testCmd = "dotnet test";
            buildCmd = "dotnet build";
            lintCmd = "dotnet format --verify-no-changes";
        }
        else if (File.Exists(Path.Combine(directory, "pyproject.toml"))
              || File.Exists(Path.Combine(directory, "setup.py"))
              || File.Exists(Path.Combine(directory, "requirements.txt")))
        {
            type = "python";
            testCmd = "pytest";
            buildCmd = "python -m build";
            lintCmd = "ruff check .";
        }
        else if (File.Exists(Path.Combine(directory, "Makefile")))
        {
            type = "make";
            testCmd = "make test";
            buildCmd = "make build";
        }
        else if (File.Exists(Path.Combine(directory, "Gemfile")))
        {
            type = "ruby";
            testCmd = "bundle exec rspec";
            buildCmd = "bundle exec rake build";
            lintCmd = "bundle exec rubocop";
        }

        // Detect source directories
        string[] candidates = ["src", "lib", "app", "tests", "test", "spec", "pkg", "cmd", "internal"];
        foreach (var dir in candidates)
        {
            if (Directory.Exists(Path.Combine(directory, dir)))
                sourceDirs.Add(dir);
        }

        return new ProjectInfo
        {
            Type = type,
            TestCommand = testCmd,
            BuildCommand = buildCmd,
            LintCommand = lintCmd,
            SourceDirs = sourceDirs,
        };
    }
}
