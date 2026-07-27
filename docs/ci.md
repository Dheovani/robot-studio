# Continuous Integration

RobotStudio uses GitHub Actions to validate the project on pushes and pull requests targeting `main` or `master`.

The workflow runs on Windows because the solution includes the first WPF desktop viewer.

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

Line endings are normalized through `.gitattributes` so Windows runners and local Windows checkouts keep text files compatible with the repository's LF formatting rule.

### Windows Installer

This job runs only for manual workflow dispatches and version tags such as `v1.0.0`.

It runs:

```bash
./scripts/release/build-windows-installer.ps1 -Version 1.0.0 -Runtime win-x64
```

It uploads these release artifacts:

- `RobotStudio-1.0.0-win-x64-setup.exe`
- `RobotStudio-1.0.0-win-x64-setup.exe.sha256`

Code signing is optional. If signing secrets are not configured, the installer is still generated unsigned.

Supported optional signing configuration:

- `ROBOTSTUDIO_SIGNING_CERTIFICATE_BASE64`: GitHub secret containing a base64-encoded `.pfx` certificate.
- `ROBOTSTUDIO_SIGNING_CERTIFICATE_PASSWORD`: GitHub secret containing the `.pfx` password.
- `ROBOTSTUDIO_SIGNING_CERTIFICATE_THUMBPRINT`: GitHub secret for signing with a certificate already available in the Windows certificate store.
- `ROBOTSTUDIO_SIGNING_TIMESTAMP_URL`: GitHub variable with a timestamp server URL.

## Current Quality Scope

The first CI version checks:

- dependency restoration;
- Release build;
- xUnit tests;
- formatting rules.
- Windows installer generation for manual or tagged release builds.

Future quality checks may include stricter analyzers, code coverage, package vulnerability scans, documentation checks, and mandatory signed release enforcement.
