# RobotStudio

RobotStudio is a didactic robotics and C#/.NET learning platform.

Version `1.2.0` adds introductory G-code, bilingual desktop resources, focused didactic tools, refined simulation workspaces, and realistic mechanical showcases for all eight available robot models.

The project is intentionally educational. It is designed to help students understand both robotics concepts and software architecture: domain modeling, motion planning, deterministic simulation, scripting, UI boundaries, tests, and future hardware integration.

## What You Can Do Now

- Simulate a generic Cartesian robot with X/Y/Z axes.
- Validate physical axis limits for position, velocity, and acceleration.
- Observe acceleration-aware movement across every available robot family using triangular or trapezoidal velocity profiles.
- Consult a searchable in-app robotics glossary covering fundamentals, motion, kinematics, simulation, programming, and safety.
- Expand an optional line-by-line G-code guide that explains units, positioning modes, homing, tool-space coordinates, feed rate, and dwell commands for the active robot.
- Model Cartesian workspace obstacles and reject linear paths that intersect them before simulated movement begins.
- Detect Differential Drive collisions using the robot's circular body footprint rather than treating its center as a dimensionless point.
- Detect SCARA collisions against both physical links throughout sampled joint-space movement, not only at the final TCP position.
- Apply deterministic spatial collision envelopes to articulated links, Delta parallel components, and the Drone body while preserving each family's own kinematics.
- Run movement commands plus `HOME`, `WAIT`, and the fault recovery command `RESET`.
- Write simple DSL scripts such as:

```txt
HOME
MOVE X=120 Y=80 Z=40 SPEED=90
DRIVE X=160 Y=80 HEADING=45 LIN=120 ANG=90
SCARA SHOULDER=45 ELBOW=30 SPEED=80
ARM BASE=45 SHOULDER=30 ELBOW=-20 SPEED=80
ARM6 J1=35 J2=30 J3=-45 J4=60 J5=20 J6=90 SPEED=80
DELTA A=30 B=60 C=90 SPEED=80
DRONE X=120 Y=80 Z=40 YAW=90 SPEED=100 YAW_SPEED=45
WAIT 500
```

- Use the introductory G-code dialect for Cartesian, SCARA, Simple Arm, Delta, and 6-DOF Industrial Arm tool-space lessons:

```gcode
G21
G90
G28
G1 X120 Y80 Z40 F5400
G91
G1 X20 Y-10
G4 P500
```

`G21` explicitly selects the project's millimeter unit; inch mode (`G20`) is rejected. `G90` selects absolute positioning and `G91` selects relative positioning. Omitted axes retain their current coordinate in absolute mode and represent zero displacement in relative mode. `F` is millimeters per minute and `G4 P` is milliseconds. The parser resolves both positioning modes into absolute `MoveToCommand` targets before validation and simulation, so the rest of the system remains independent of G-code.

RobotStudio treats G-code coordinates as TCP tool-space coordinates, never as aliases for physical joints or actuators. Cartesian Robot and XY Plotter map coordinates directly. SCARA supports planar `G1 X/Y` through deterministic elbow-down inverse kinematics. Simple Articulated Arm supports planar tool poses with `G1 X/Y/A`. Delta supports `G1 X/Y/Z` through exact inverse kinematics. The 6-DOF Industrial Arm accepts `G1 X/Y/Z/A/B/C` through deterministic positive-elbow/wrist-neutral inverse kinematics and continuous tool-pose playback. Its introductory topology couples yaw `C` to the `X/Y` position azimuth and reports incompatible poses explicitly. Differential Drive and Drone use their robot-appropriate Simple DSL commands instead.

`RESET` acknowledges a fault when execution resumes from a failed simulation context. It returns the logical state to `Idle` while preserving the robot's physical state and elapsed simulation time; `HOME` remains the recovery option that physically returns the robot to its family-specific origin.

- Run the CLI to inspect commands, timeline steps, final state, and final position.
- Open the WPF desktop viewer to inspect the Cartesian robot in 3D.
- Open the first XY Plotter viewer as a beginner two-axis model.
- Open the Differential Drive viewer for an intermediate mobile-robot simulation.
- Inspect ideal differential-drive odometry with accumulated wheel travel and rotation.
- Open the first SCARA viewer for introductory articulated joint-space simulation.
- Open the first Simple Articulated Arm viewer for three-joint articulated robot lessons.
- Open the first Delta Robot viewer for simplified parallel-actuator simulation.
- Open the Drone viewer for simplified 3D position plus coordinated roll, pitch, and yaw attitude simulation.
- Load local teaching examples from every available desktop viewer.
- Load and save `.robot`, `.gcode`, or `.txt` scripts in the desktop app.
- Use keyboard shortcuts for active viewer playback, frame stepping, simulation, script files, zoom, and 3D camera controls; script validation runs automatically while editing.
- Read clearer validation summaries when scripts contain syntax errors, invalid arguments, or physical limit violations.
- Rotate, zoom, and reset the camera.
- Use manual jog buttons and a direct command console.
- Configure Cartesian X/Y/Z limits, maximum velocity, and maximum acceleration directly in the desktop workspace.
- Inspect playback frames, state, position, exact velocity and acceleration charts, planned path, workspace, TCP, and didactic tooltips.
- Export and validate playback snapshots as JSON.

## Current Release

Current stable version: `1.2.0`.

This release provides a stable educational progression from Cartesian motion to mobile, articulated, parallel, and aerial robotics without requiring real hardware.

Implemented:

- domain model;
- Cartesian robot profile;
- motion planner;
- deterministic simulator;
- simple DSL;
- CLI workflow;
- desktop 3D viewer;
- robot catalog metadata;
- first XY Plotter domain, motion, and viewer path;
- Differential Drive domain, motion planner, deterministic simulator, and 2D viewer;
- SCARA domain, kinematics, motion planner, deterministic simulator, DSL support, playback sampler, and 3D viewer;
- Simple Articulated Arm domain, forward kinematics, motion planner, deterministic simulator, DSL support, playback sampler, and 3D viewer;
- Delta Robot domain, simplified parallel kinematics, motion planner, deterministic simulator, DSL support, playback sampler, and 3D viewer;
- Drone domain, 3D pose and attitude model, coordinated motion planner, deterministic simulator, DSL support, playback sampler, and 3D viewer;
- 6-DOF Industrial Arm domain, forward and deterministic inverse kinematics, joint and linear tool-pose planners, `ARM6` DSL and `G1 X/Y/Z/A/B/C` support, deterministic simulator, playback sampler, local examples, and 3D viewer;
- shared desktop rendering helpers for orbit cameras, simple meshes, paths, and reachable workspaces;
- shared playback contracts for cross-family simulation summaries;
- local desktop teaching examples and selectors for available training viewers;
- playback snapshots;
- didactic overlays, charts, timeline, and tooltips;
- Simple DSL and an introductory robot-mapped G-code dialect;
- future hardware boundaries.

Not implemented yet:

- real serial communication;
- Arduino or ESP32 firmware/protocols;
- richer industrial-arm graphics, inverse kinematics, singularity analysis, and collision visualization.
- planned Cylindrical, Ackermann Steering, Omnidirectional, Self-Balancing, Stewart Platform, and Mobile Manipulator simulations.

## Run The Desktop App

Requirements:

- Windows;
- .NET SDK matching `global.json`.

From the repository root:

```bash
dotnet run --project src/RobotStudio.Desktop
```

The desktop app starts with a robot selection screen. `Cartesian Robot`, `XY Plotter`, `Differential Drive Robot`, `SCARA Robot`, `Simple Articulated Arm`, `Delta Robot`, `Drone`, and `6-DOF Industrial Arm` are available in version `1.2.0`.

The Cartesian movement explanation panel identifies triangular and trapezoidal profiles, velocity and acceleration limits, phase durations, and the active playback phase using exact simulation metadata.

## Windows CLI And Core

RobotStudio currently targets Windows because the desktop viewer uses WPF and the official release tooling is Windows-based. The CLI, domain, motion, simulation, scripting, hardware boundary, and tests are validated through the main solution.

Build the solution:

```bash
dotnet build RobotStudio.slnx
```

Run tests:

```bash
dotnet test RobotStudio.slnx
```

Build a Windows CLI release artifact:

```bash
powershell -ExecutionPolicy Bypass -File scripts/release/build-cli-artifact.ps1 -Version 1.2.0 -Runtime win-x64
```

Supported CLI release runtime:

- `win-x64`

## Application Tour

### Robot Catalog

Choose an available simulator or explore its mechanical construction. Planned models remain visible as future learning paths.

![RobotStudio robot catalog](docs/assets/screenshots/robot-catalog.png)

### Schematic Simulation

Write or load a script, generate deterministic playback, and inspect the robot's movement in its didactic workspace.

![RobotStudio Cartesian simulator and script workspace](docs/assets/screenshots/cartesian-simulator.png)

### State And Motion Monitoring

Follow the current frame, robot state, position, velocity, acceleration, and motion charts while scrubbing or playing the timeline.

![RobotStudio simulation monitor](docs/assets/screenshots/simulation-monitor.png)

### Mechanical Exploration

Use the separate mechanical showcase to inspect meaningful components, their relationships, and curated demonstrations without replacing the schematic simulator.

![RobotStudio Cartesian mechanical showcase](docs/assets/screenshots/mechanical-showcase.png)

### Robotics Glossary

Search the built-in glossary by term or topic without leaving the application.

![RobotStudio robotics glossary](docs/assets/screenshots/robotics-glossary.png)

## Run The CLI

Run the built-in simulation example:

```bash
dotnet run --project src/RobotStudio.Cli
```

Other useful CLI commands:

```bash
dotnet run --project src/RobotStudio.Cli -- example
dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian/basic.robot
dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian/basic.robot
dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian/basic.gcode
dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian/basic.gcode
dotnet run --project src/RobotStudio.Cli -- simulate lesson.txt --dialect gcode
dotnet run --project src/RobotStudio.Cli -- playback examples/cartesian/basic.robot 500
dotnet run --project src/RobotStudio.Cli -- export-playback examples/cartesian/basic.robot 500 playback.json
dotnet run --project src/RobotStudio.Cli -- validate-playback playback.json
```

The CLI infers G-code from `.gcode` and Simple DSL from `.robot`. For `.txt` files or an intentional override, pass `--dialect dsl` or `--dialect gcode`. The option is supported by `example`, `validate`, `simulate`, `playback`, and `export-playback`.

The [`examples`](examples/README.md) directory is organized by robot model. Cartesian teaching files include an intentional axis-limit failure, requested-versus-effective speed comparisons, relative positioning, and a jog-style wait/home sequence.

## Build And Test

Build:

```bash
dotnet build
```

Run tests:

```bash
dotnet test
```

Check formatting:

```bash
dotnet format RobotStudio.slnx --verify-no-changes
```

## Build The Windows Installer

RobotStudio `1.2.0` is distributed as a Windows installer for the WPF desktop app.

```bash
powershell -ExecutionPolicy Bypass -File scripts/release/build-windows-installer.ps1 -Version 1.2.0 -Runtime win-x64
```

The installer is generated at:

```txt
artifacts/release/RobotStudio-1.2.0-win-x64-setup.exe
```

The release script also generates:

```txt
artifacts/release/RobotStudio-1.2.0-win-x64-setup.exe.sha256
```

When a version tag such as `v1.2.0` is pushed to GitHub, CI derives the artifact version from the tag, builds the Windows installer and Windows CLI ZIP archive, and publishes a GitHub Release with all `.exe`, `.zip`, and `.sha256` assets attached.

## Project Structure

- `src/RobotStudio.Domain`: pure domain models, commands, states, limits, kinematics inputs, contracts, and errors for the supported robot families.
- `src/RobotStudio.Motion`: family-appropriate deterministic motion planning with coordinated acceleration-aware profiles.
- `src/RobotStudio.Simulation`: deterministic command execution, sampling, playback snapshots, visual states, and scene frames.
- `src/RobotStudio.Scripting`: Simple DSL plus semantic G-code parsing and robot-specific command mapping exposed through a shared dialect contract.
- `src/RobotStudio.Hardware`: future hardware integration boundary contracts and planned prototype metadata.
- `src/RobotStudio.Visualization`: renderer-neutral visual-model hierarchies, semantic robot parts, component poses, and curated mechanical demonstrations.
- `src/RobotStudio.Cli`: terminal entry point for examples, validation, simulation, playback, and snapshot export.
- `src/RobotStudio.Desktop`: WPF desktop app for robot selection, schematic simulation, and isolated mechanical showcases.
- `tests`: xUnit test projects for domain, motion, simulation, scripting, hardware boundaries, and desktop metadata/tooling.
- `docs`: product, architecture, use case, testing, CI, and user documentation.

## Documentation

- [Changelog](CHANGELOG.md)
- [Documentation Index](docs/README.md)
- [Technical Decisions](docs/technical-decisions.md)
- [Advanced 3D Visualization](docs/advanced-3d-visualization.md)
- [Use Cases](docs/use-cases.md)
- [Test Map](docs/test-map.md)
- [User Guide](docs/user-guide.md)
- [Continuous Integration](docs/ci.md)
- [Contributing](CONTRIBUTING.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)
- [Security Policy](SECURITY.md)

## License

RobotStudio is proprietary software. Personal, non-commercial study use is allowed under the [RobotStudio Personal Study License](LICENSE).

Commercial, business, organizational, institutional, brand-related, redistribution, sublicensing, and public hosting uses are not allowed without prior written permission from the copyright holder.
