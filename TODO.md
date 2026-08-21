# TODO

This file tracks future work after the `1.1.0` multi-robot teaching platform release. Completed release history belongs in `CHANGELOG.md`.

## 1. Release And Distribution

- [x] Create and push the `v1.1.0` Git tag after the release commit passes CI.
- [ ] Acquire a code signing certificate before publishing signed Windows releases.
- [ ] Add Windows ARM64 CLI release artifacts if student machines need them.

## 2. Scripting And G-Code

- [x] Add a script dialect selector to the Cartesian and XY Plotter desktop workspaces.
- [x] Design the first conservative G-code parser dialect with explicit coordinates before introducing modal positioning state.
- [x] Map `G28` to homing behavior.
- [x] Map `G1 X... Y... Z... F...` to linear movement, interpreting `F` as millimeters per minute.
- [x] Map `G4 P...` to wait/dwell behavior, interpreting `P` as milliseconds.
- [x] Allow Simple DSL and G-code scripts to produce the same domain command model.
- [x] Add equivalent `examples/cartesian/basic.robot` and `examples/cartesian/basic.gcode` programs.
- [x] Add dialect-aware CLI validation, simulation, playback, export, and examples with extension inference and an explicit `--dialect` override.
- [x] Add `G90` absolute and `G91` relative positioning with parse context, retained coordinates, and deterministic conversion to absolute `MoveToCommand` targets.
- [x] Replace the Cartesian-specific shared parse context with an extensible robot-position context and explicit compatibility validation.
- [x] Preserve executable commands and non-executable modal directives in a compiled script representation.
- [x] Carry requested-motion metadata through Cartesian simulation and playback so the desktop does not reparse isolated source lines.
- [ ] Expand G-code beyond Cartesian motion only when a standard, robot-appropriate mapping is selected.

## 3. Hardware Integration

- [ ] Define the first serial protocol draft.
- [ ] Choose the first supported educational controller board.
- [ ] Choose the first supported motor driver setup.
- [ ] Implement serial port discovery.
- [ ] Implement connection open, close, and health checks.
- [ ] Implement command transmission in dry-run mode before enabling real motion.
- [ ] Add hardware safety limits before any real execution path.
- [ ] Add Arduino or ESP32 firmware examples only after the protocol is stable.

## 4. Didactic Tools

- [x] Expand movement explanations for acceleration-aware plans.
- [x] Add lesson-friendly examples for invalid axis limits.
- [x] Add examples for requested speed versus effective speed.
- [x] Add examples for waits, homing, manual jogging, and command sequencing.
- [x] Add glossary entries for technical robotics terms.
- [ ] Add optional inline explanations for future G-code commands.

## 5. Testing And Quality

- [ ] Add code coverage reporting when coverage goals are defined.
- [ ] Add stricter analyzers when the coding standard becomes more mature.
- [ ] Add package vulnerability scanning when external dependencies become more relevant.
- [ ] Add UI smoke tests if the desktop workflow becomes stable enough to automate.

## 6. Advanced 3D Visualization And Realistic Robot Rendering

Milestone 6 is future work. Its architecture and implementation constraints are specified in [Advanced 3D Visualization](docs/advanced-3d-visualization.md). Do not add a rendering dependency before this milestone begins and its library evaluation is revalidated.

- [ ] Revalidate renderer capabilities, maintenance status, platform support, and licenses; investigate the appropriate HelixToolkit integration first if WPF remains the desktop framework.
- [ ] Define renderer-neutral visual-state, component-pose, semantic part identifier, and robot visual-model contracts without adding graphics-library types to Domain, Motion, or Simulation.
- [ ] Extract robot-specific schematic scene composition and viewport lifecycle code from `MainWindow.xaml.cs` behind desktop rendering interfaces.
- [ ] Define a versioned asset manifest direction that maps glTF 2.0/GLB nodes to RobotStudio semantic parts without locking the project into a premature schema.
- [ ] Implement model loading, asset validation, caching, mesh and material reuse, and deterministic failure reporting for missing or incompatible assets.
- [ ] Implement a realistic renderer proof of concept for one existing robot while preserving the current schematic/didactic renderer.
- [ ] Add visualization mode selection for schematic, realistic, and realistic with educational overlays.
- [ ] Add semantic component selection, highlighting, and educational inspection backed by RobotStudio part identifiers rather than raw mesh identifiers.
- [ ] Compose axes, workspace, trajectory, coordinate systems, labels, limits, and future collision bounds independently of the selected robot renderer.
- [ ] Separate deterministic simulation ticks from rendering frames and define interpolation, transform-update, and scene-update policies.
- [ ] Measure model loading, rendering, transform updates, and hit-testing performance on representative teaching hardware.
- [ ] Add architecture, asset-contract, mode-switching, selection-mapping, and rendering smoke tests before expanding realistic assets to additional robot families.

## 7. Future Robot Family Expansion

- [ ] Implement the Cylindrical Robot as the first mixed revolute/prismatic teaching model.
- [ ] Implement the Ackermann Steering Robot for car-like steering geometry and non-holonomic motion.
- [ ] Implement the Omnidirectional Robot for holonomic movement and wheel-speed decomposition.
- [ ] Implement the Self-Balancing Robot after dynamic simulation, sensors, and feedback-control infrastructure exist.
- [ ] Implement the Stewart Platform as the advanced six-actuator parallel mechanism.
- [ ] Implement the Mobile Manipulator as a capstone that coordinates a mobile base and articulated arm.
- [ ] Expand the catalog by mapping new robots for implementation.
- [ ] Add tests before each new robot family is considered complete.
