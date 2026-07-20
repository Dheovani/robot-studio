# Contributing

Thank you for your interest in RobotStudio.

RobotStudio is proprietary software shared for personal, non-commercial study
use. Contributions are welcome only when they support the educational goals of
the project and comply with the license.

## License Notice

By submitting a contribution, you agree that:

- your contribution may be used, modified, and incorporated by the project owner;
- your contribution does not grant you ownership of the project;
- your contribution does not change the project license;
- your contribution does not grant commercial, organizational, institutional,
  brand-related, redistribution, sublicensing, or trademark rights;
- you have the right to submit the contribution.

Do not submit code, documentation, assets, or examples that you do not have the
right to contribute.

## Development Principles

Contributions should follow the current direction of the project:

- keep the domain model clean and deterministic;
- keep UI, hardware, and infrastructure concerns out of `RobotStudio.Domain`;
- prefer readable, modern C#;
- prefer small, testable changes;
- avoid unnecessary external packages;
- keep documentation in English;
- keep the project suitable for learners.

## Before Submitting Changes

Run these commands from the repository root:

```bash
dotnet format RobotStudio.slnx --verify-no-changes
dotnet build
dotnet test
dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian.robot
```

If a command fails because of local SDK or environment issues, explain the
failure clearly.

## Pull Request Guidelines

Pull requests should:

- describe the purpose of the change;
- include tests when behavior changes;
- update documentation when concepts, commands, or workflows change;
- stay focused on one topic;
- avoid unrelated formatting churn.

## Not Accepted Without Prior Approval

Please do not submit changes that add:

- desktop UI frameworks;
- 3D rendering libraries;
- hardware communication;
- Arduino or ESP32 integration;
- G-code support;
- external dependencies;
- commercial, branded, institutional, or organizational use cases.

These areas may be developed later, but only when they fit the project roadmap.
