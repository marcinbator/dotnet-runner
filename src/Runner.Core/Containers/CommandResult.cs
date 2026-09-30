namespace Runner.Core.Containers;

public sealed record CommandResult(long ExitCode, string Stdout, string Stderr);
