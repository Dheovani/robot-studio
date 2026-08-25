# RobotStudio Documentation

This folder keeps the project decisions and learning-oriented documentation close to the code. All documentation must stay in English.

## Documents

- [Technical Decisions](technical-decisions.md): architectural and product decisions that should guide implementation.
- [Use Cases](use-cases.md): user-facing workflows the application must eventually support.
- [Test Map](test-map.md): expected automated test coverage by project area.
- [User Guide](user-guide.md): current and planned ways to use the CLI and future scripting flow.
- [Robotics Glossary](glossary.md): technical robotics, programming, motion, simulation, and safety terms used by RobotStudio.
- [Continuous Integration](ci.md): GitHub Actions validation for build, tests, and formatting.
- [Advanced 3D Visualization](advanced-3d-visualization.md): Milestone 6 rendering modes, mechanical showcase scope, technology constraints, asset direction, and architectural boundaries.
- [Renderer Evaluation](renderer-evaluation.md): Milestone 6 renderer choice, risks, asset pipeline, and proof-of-concept acceptance criteria.
- [Visual Asset Manifest](visual-asset-manifest.md): version 1 GLB package metadata, semantic node mapping, validation, and ownership boundaries.
- [Changelog](../CHANGELOG.md): user-facing release history and included capabilities.

## Maintenance Rules

- Update these documents when a product rule, architectural boundary, or user workflow changes.
- Keep `TODO.md` as the single source for future pending work.
- Keep docs concise, specific, and aligned with implemented behavior.
- Do not document future features as available before they are implemented.
