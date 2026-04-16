using Rwl.Commands;
using Spectre.Console.Cli;

// Show the banner when invoked with no args or a top-level help flag
if (args.Length == 0 || (args.Length == 1 && args[0] is "--help" or "-h"))
    Banner.Show();

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("rwl");
    config.SetApplicationVersion("2.0.0");

    config.AddCommand<InitCommand>("init")
        .WithDescription("Initialize Ralph Wiggum Loop in current project")
        .WithExample("init")
        .WithExample("init", "--no-hooks");

    config.AddCommand<DoctorCommand>("doctor")
        .WithDescription("Verify loop setup and report issues");

    config.AddCommand<StatusCommand>("status")
        .WithDescription("Show task progress dashboard");

    config.AddCommand<PlanCommand>("plan")
        .WithDescription("Interactive task planner");

    config.AddCommand<AddTaskCommand>("add-task")
        .WithDescription("Add a single task to TASKS.md")
        .WithExample("add-task")
        .WithExample("add-task", "Fix auth module");

    config.AddCommand<RunCommand>("run")
        .WithDescription("Start the iterative loop");

    config.AddCommand<RunOneCommand>("run-one")
        .WithDescription("Execute a single loop iteration");

    config.AddCommand<StopCommand>("stop")
        .WithDescription("Create stop flag to halt the loop");

    config.AddCommand<CompactCommand>("compact")
        .WithDescription("Compact PROGRESS.md to reduce size")
        .WithExample("compact")
        .WithExample("compact", "10");

    config.AddCommand<ResetCommand>("reset")
        .WithDescription("Reset loop state (tasks, progress, or all)");

    config.AddCommand<HealthCommand>("health")
        .WithDescription("Run convergence and guardrail checks");
});

return await app.RunAsync(args);
