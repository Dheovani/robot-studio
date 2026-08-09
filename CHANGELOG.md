# Changelog

All notable user-facing changes to RobotStudio are documented in this file.

## Unreleased - Multi-Robot Teaching Platform Expansion

### Added

- Added a didactic robot catalog organized by learning complexity, with available and planned robot templates.
- Added planned catalog descriptors for the Cylindrical Robot, Ackermann Steering Robot, Omnidirectional Robot, Self-Balancing Robot, Stewart Platform, and Mobile Manipulator.
- Added the XY Plotter as a beginner Cartesian-family model for two-axis drawing and planar command sequencing.
- Added the Differential Drive Robot as the first mobile robot model, including deterministic motion, DSL commands, playback, and a desktop viewer.
- Added the SCARA Robot as an articulated robot model, including joint-space motion, kinematics, DSL commands, playback, and a 3D desktop viewer.
- Added the Simple Articulated Arm as a three-joint arm model, including forward kinematics, DSL commands, playback, and a 3D desktop viewer.
- Added the Delta Robot as the first parallel robot model, including simplified parallel kinematics, actuator-space commands, playback, and a 3D desktop viewer.
- Added the Drone as the first aerial robot model, including 3D pose, yaw orientation, coordinated flight planning, DSL commands, playback, and a 3D desktop viewer.
- Added the 6-DOF Industrial Arm, including six-joint limits, simplified forward kinematics, coordinated joint planning, the `ARM6` DSL command, deterministic simulation, playback sampling, local examples, didactic state presentation, and a 3D desktop viewer.
- Added shared playback snapshot and frame contracts so desktop and future tooling can consume different robot families without forcing them into one position model.
- Added reusable desktop rendering helpers for orbit cameras, mesh primitives, reachable workspaces, paths, and volumetric robot parts.
- Added local desktop teaching examples and selectors for every available robot viewer.
- Added desktop script load/save support for `.robot` and `.txt` files.
- Added clearer script validation messages for syntax errors, unsupported command arguments, and physical limit violations.
- Added keyboard shortcuts for active-viewer script loading, saving, validation, simulation, playback, frame stepping, zoom, and camera reset.
- Added architecture tests to guard project dependency rules and keep WPF isolated from portable core projects.
- Added portable solution validation for non-desktop projects, keeping the CLI and core libraries testable outside the Windows desktop target.
- Added Windows release artifact and installer generation groundwork for distributing the desktop application.
- Added reusable trapezoidal and triangular motion profiles with deterministic acceleration, constant-velocity, and deceleration sampling.
- Added Cartesian playback snapshot format 2 with exact motion phase, velocity, and acceleration metrics while retaining format 1 validation compatibility.
- Added Cartesian profile phase, velocity, and acceleration state values plus an acceleration/deceleration chart in the desktop viewer.
- Added explicit angular acceleration limits and synchronized motion profiles for the SCARA, Simple Articulated Arm, and 6-DOF Industrial Arm.
- Added independent linear and angular acceleration limits and motion profiles for the Differential Drive Robot.
- Added synchronized acceleration-aware actuator profiles for the Delta Robot.
- Added synchronized 3D translation and yaw acceleration profiles for the Drone.
- Added ideal Differential Drive odometry with accumulated left/right wheel travel and rotation in playback and didactic state presentation.
- Added Drone roll and pitch limits, commands, synchronized attitude planning, playback, 3D model inclination, and DSL support alongside yaw.
- Added the cross-family `RESET` command for recovering faulted simulations without changing robot pose, joint/actuator state, odometry, or simulated time.

### Changed

- Expanded the desktop app from a Cartesian-only visual simulator into a multi-robot teaching environment.
- Updated Cartesian and XY plotter planning to respect acceleration limits, producing acceleration-aware durations and Cartesian playback interpolation.
- Updated articulated playback to follow acceleration-aware profile progress instead of constant-speed joint interpolation.
- Updated differential-drive playback to complete its planned translation before rotation and to follow each segment's acceleration-aware progress.
- Updated Delta and Drone playback to follow deterministic profile progress instead of constant-speed interpolation.
- Kept G-code, hardware execution, real drone physics, collision detection, inverse kinematics, and richer 3D graphics as planned future work.

## 1.0.0 - Stable Cartesian Simulation Release

RobotStudio `1.0.0` is the first stable educational release of the project. It provides a complete introductory robotics workflow centered on a Cartesian X/Y/Z robot, while keeping the architecture prepared for future robot families.

This release is suitable for studying the current simulator, running scripts, inspecting deterministic playback data, and using the first desktop 3D viewer.

### What Is Included

- A clean C#/.NET solution organized into domain, motion, simulation, scripting, hardware boundary, CLI, desktop, and test projects.
- A pure domain model for robot concepts, Cartesian positions, robot states, commands, axis limits, and domain validation.
- A simple linear motion planner that estimates distance, effective velocity, duration, involved axes, and speed limiting.
- A deterministic simulation engine for `HOME`, `MOVE`, and `WAIT` commands.
- A beginner-friendly DSL for writing robot command scripts.
- A CLI workflow for validating scripts, running simulations, exporting playback snapshots, and validating exported snapshots.
- A WPF desktop application with robot selection, Cartesian 3D visualization, orbit camera, zoom, playback controls, manual jog controls, command console, overlays, charts, timeline markers, and didactic explanations.
- A robot catalog that lists the current Cartesian robot and planned future robot families without implementing them prematurely.
- Hardware integration contracts prepared for future serial communication, without real Arduino, ESP32, or device execution yet.
- A script dialect boundary prepared for future G-code support, without implementing G-code parsing yet.
- Automated tests covering domain rules, motion planning, scripting, simulation, playback snapshots, desktop metadata, didactic tooltips, and hardware boundary contracts.
- GitHub Actions CI for restore, build, tests, formatting verification, and test result artifacts.
- Project documentation, including technical decisions, use cases, test map, CI behavior, user guide, contribution policy, security policy, code of conduct, license, and roadmap.

### Current Desktop Experience

- Select the available Cartesian robot from the start screen.
- Inspect the robot in a 3D viewport.
- Rotate the camera by dragging with the mouse.
- Zoom with the mouse wheel or camera controls.
- Run the built-in DSL script.
- Edit scripts directly inside the desktop app.
- Validate, simulate, play, pause, reset, step through frames, and scrub playback.
- Operate the robot manually with jog buttons.
- Execute direct DSL commands from the command console.
- Toggle didactic overlays such as workspace, grid, axes, labels, TCP, path, markers, rails, and components.
- Inspect robot state, position, timing, velocity, movement explanation, charts, and script line mapping.

### Not Included Yet

- Additional implemented robot families.
- G-code parsing or execution.
- Real serial communication.
- Arduino or ESP32 firmware.
- Real hardware control from the desktop app.
- Inverse kinematics.
- Drone physics.
- Collision detection.
- Packaging or installer generation.
- A learning management system.

Future work is tracked in `TODO.md`.
