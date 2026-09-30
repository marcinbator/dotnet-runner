# dotnet-runner

A github-actions-style Runner implementation in C#.

Jobs are defined in YAML (samples in `definitions` dir) and steps run inside Docker container:

```yaml
name: test
image: alpine:3.20

steps:
  - id: s1
    name: step1
    run: echo "hello"
```

## Requirements

- .NET 10 SDK
- Docker

## Usage

```sh
docker compose up --build
```

or locally:

```sh
dotnet run --project src/Runner.Host
```

Job definitions are loaded from `definitions/` (configurable via `Runner:DefinitionsPath`).

## Tests

```sh
dotnet test
```
