# Continuous Integration

RobotStudio uses GitHub Actions to validate the project on pushes and pull requests targeting `main` or `master`.

The current CI is Windows-only because the desktop app uses WPF and the official release tooling builds Windows artifacts.

## Workflow

The workflow file is:

```txt
.github/workflows/ci.yml
```

## Jobs

### Release Metadata

For manual release runs and version tags, this job resolves one validated semantic version for every packaging job. Tags such as `v1.2.0` become artifact version `1.2.0`; manual runs use the required `version` workflow input.

### Windows Desktop Build And Test

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

This job runs only for manual workflow dispatches and version tags such as `v1.2.0`.

It runs:

```bash
./scripts/release/build-windows-installer.ps1 -Version <resolved-version> -Runtime win-x64
```

It uploads these release artifacts:

- `RobotStudio-<version>-win-x64-setup.exe`
- `RobotStudio-<version>-win-x64-setup.exe.sha256`

For tagged builds, `<version>` is derived from the tag by removing its leading `v`. Manual workflow runs use the required `version` input.

Code signing is optional. If signing secrets are not configured, the installer is still generated unsigned.

Supported optional signing configuration:

- `ROBOTSTUDIO_SIGNING_CERTIFICATE_BASE64`: GitHub secret containing a base64-encoded `.pfx` certificate.
- `ROBOTSTUDIO_SIGNING_CERTIFICATE_PASSWORD`: GitHub secret containing the `.pfx` password.
- `ROBOTSTUDIO_SIGNING_CERTIFICATE_THUMBPRINT`: GitHub secret for signing with a certificate already available in the Windows certificate store.
- `ROBOTSTUDIO_SIGNING_TIMESTAMP_URL`: GitHub variable with a timestamp server URL.

### Windows CLI Release Artifact

This job runs only for manual workflow dispatches and version tags such as `v1.2.0`.

It builds a self-contained CLI ZIP archive for:

- `win-x64`

The archive is uploaded with a matching `.sha256` checksum file.

### GitHub Release

This job runs only for version tags such as `v1.2.0`.

It waits for the Windows installer and Windows CLI artifact jobs, downloads the workflow artifacts, keeps only public release assets, and creates the GitHub Release for the tag.

Published assets include:

- Windows installer `.exe`;
- Windows installer `.sha256`;
- Windows CLI ZIP archive;
- Windows CLI `.sha256` file.

The release notes are extracted from the matching version section in `CHANGELOG.md`. Publication fails if that section is missing.

## Current Quality Scope

The CI checks:

- dependency restoration;
- Windows Release build;
- xUnit tests;
- architecture dependency rules;
- formatting rules;
- Windows installer generation for manual or tagged release builds;
- Windows CLI artifact generation for manual or tagged release builds;
- GitHub Release publication for version tags.

Future quality checks may include stricter analyzers, code coverage, package vulnerability scans, documentation checks, and mandatory signed release enforcement.
