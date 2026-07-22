# Changelog

All notable user-facing changes to RobotStudio are documented in this file.

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
