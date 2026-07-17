# Continuous Integration

RobotStudio uses GitHub Actions to validate the project on pushes and pull requests targeting `main` or `master`.

## Workflow

The workflow file is:

```txt
.github/workflows/ci.yml
```

## Jobs

### Build And Test

This job runs:

```bash
dotnet restore RobotStudio.slnx
dotnet build RobotStudio.slnx --configuration Release --no-restore
dotnet test RobotStudio.slnx --configuration Release --no-build
```

It also uploads `.trx` test result files as workflow artifacts.

### Code Quality

This job runs:

```bash
dotnet restore RobotStudio.slnx
dotnet format RobotStudio.slnx --verify-no-changes --verbosity diagnostic
```

The goal is to keep formatting aligned with `.editorconfig`.

## Current Quality Scope

The first CI version checks:

- dependency restoration;
- Release build;
- xUnit tests;
- formatting rules.

Future quality checks may include stricter analyzers, code coverage, package vulnerability scans, and documentation checks.
