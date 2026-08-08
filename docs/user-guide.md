# User Guide

## Current Requirements

- .NET SDK matching `global.json`.
- A terminal capable of running `dotnet` commands.

## Build The Project

Run from the repository root:

```bash
dotnet build
```

The full solution includes the WPF desktop viewer and is intended for Windows.

Build the portable CLI/core solution on Windows, Linux, or macOS:

```bash
dotnet build build/RobotStudio.Portable.slnx
```

## Run Tests

Run from the repository root:

```bash
dotnet test
```

Run only the portable tests on Windows, Linux, or macOS:

```bash
dotnet test build/RobotStudio.Portable.slnx
```

Build a portable CLI release artifact:

```bash
powershell -ExecutionPolicy Bypass -File scripts/release/build-cli-artifact.ps1 -Version 1.0.0 -Runtime linux-x64
```

The initial CLI release runtimes are:

- `win-x64`
- `linux-x64`
- `osx-x64`

CLI release archives are generated under:

```txt
artifacts/release/
```

## Run The CLI Example

Run from the repository root:

```bash
dotnet run --project src/RobotStudio.Cli
```

The default CLI run uses the built-in example script, executes it through the simulator, and prints the result.

## Run The Desktop Viewer

Run from the repository root on Windows:

```bash
dotnet run --project src/RobotStudio.Desktop
```

The desktop app opens a WPF window with a robot selection screen. The Cartesian robot, XY plotter, differential drive robot, SCARA robot, Simple Articulated Arm, Delta Robot, Drone, and 6-DOF Industrial Arm are available now. Cylindrical, Ackermann Steering, Omnidirectional, Self-Balancing, Stewart Platform, and Mobile Manipulator templates appear as planned learning paths and cannot be opened yet.

Opening the Cartesian robot renders the built-in Cartesian simulation in a 3D viewport and provides playback, camera controls, and a local example selector.

Opening the XY plotter renders a beginner two-axis drawing model on a fixed `Z=0` drawing plane. It uses X/Y movement while reusing the same script validation, playback, timeline, chart, overlay controls, and local example selector.

Opening the differential drive robot renders a 2D mobile robot viewer with workspace grid, playback path, robot body, wheels, heading indicator, current pose, command name, and timeline controls. The viewer includes a mobile DSL editor for `HOME`, `DRIVE`, and `WAIT` commands, plus a local example selector.

Opening the SCARA robot renders a 3D articulated robot viewer with reachable workspace, volumetric base, shoulder joint, elbow joint, tool point, planned path, current joint angles, current tool pose, command name, camera orbit, zoom, and timeline controls. The viewer includes a SCARA DSL editor for `HOME`, `SCARA`, and `WAIT` commands, plus a local example selector.

Opening the Simple Articulated Arm renders a 3D three-joint arm viewer with reachable workspace, volumetric base, base joint, shoulder, elbow, tool point, tool orientation, planned path, current joint angles, current tool pose, command name, camera orbit, zoom, and timeline controls. The viewer includes an ARM DSL editor for `HOME`, `ARM`, and `WAIT` commands, plus a local example selector.

Opening the Delta Robot renders a 3D simplified parallel robot viewer with a triangular frame, three vertical actuators, moving carriages, parallel links, platform/TCP, reachable workspace, planned path, current actuator positions, current tool pose, command name, camera orbit, zoom, and timeline controls. The viewer includes a Delta DSL editor for `HOME`, `DELTA`, and `WAIT` commands, plus a local example selector.

Opening the Drone renders a 3D aerial robot viewer with flight-volume boundaries, ground grid, drone body, rotor arms, yaw direction indicator, planned path, current X/Y/Z position, current yaw, command name, camera orbit, zoom, and timeline controls. The viewer includes a Drone DSL editor for `HOME`, `DRONE`, and `WAIT` commands, plus a local example selector.

Opening the 6-DOF Industrial Arm renders a 3D serial arm viewer with a raised base, six joint markers, volumetric links, wrist/tool orientation, reachable floor area, TCP path, joint state, command name, camera orbit, zoom, and timeline controls. The viewer includes an industrial-arm DSL editor for `HOME`, `ARM6`, and `WAIT` commands, plus local examples.

Every available desktop viewer includes an example selector and a `Load Example` button. The non-Cartesian side panels also explain current movement concepts where that viewer already has a didactic explanation panel.

Script editors in the desktop app can load and save local `.robot` or `.txt` files. Loading a script replaces the editor text and asks the student to validate or simulate before playback. Saving writes the current editor text without changing the simulation.

When validation fails, the desktop app shows a student-facing summary. Syntax errors include the script line number when available, physical limit errors explain that the target is outside the workspace, and command argument errors suggest checking required values such as speed or duration.

Desktop keyboard shortcuts:

- `Ctrl+O`: load a script into the active viewer.
- `Ctrl+S`: save the active viewer script.
- `Ctrl+Enter`: validate the active script.
- `F5`: simulate the active script.
- `Space`: play or pause playback when focus is not inside an editor.
- `Left` / `Right`: move one frame backward or forward when focus is not inside an editor.
- `Ctrl+R`: reset playback to the first frame.
- `Ctrl++` / `Ctrl+-`: zoom the active viewer when focus is not inside an editor.
- `Ctrl+0`: reset the active viewer zoom or camera when focus is not inside an editor.
- `Ctrl+mouse wheel`: zoom the active viewer under the mouse pointer.

## Build The Windows Installer

Run from the repository root on Windows:

```bash
powershell -ExecutionPolicy Bypass -File scripts/release/build-windows-installer.ps1 -Version 1.0.0 -Runtime win-x64
```

The installer is generated at:

```txt
artifacts/release/RobotStudio-1.0.0-win-x64-setup.exe
```

The script also generates a SHA256 checksum file next to the installer:

```txt
artifacts/release/RobotStudio-1.0.0-win-x64-setup.exe.sha256
```

For official releases, push a version tag such as `v1.0.0`. GitHub Actions will publish a GitHub Release with the Windows installer, portable CLI ZIP archives, and SHA256 checksum files attached.

Current desktop controls:

- robot cards showing name, family, status badge, complexity badge, description, and capability tags.
- robot selection cards arrange responsively across one, two, or three columns depending on window width.
- robot selection cards show hover and keyboard focus feedback.
- `Open Robot` on the Cartesian robot card.
- disabled planned robot entries ordered by didactic complexity.
- `Robots` inside the Cartesian viewer to return to the selection screen.
- DSL editor inside the Cartesian viewer.
- script editor gutter with line numbers and command tags for `HOME`, `MOVE`, and `WAIT`.
- collapsible sidebar panels for script, manual control, command console, robot state, charts, movement explanation, timeline markers, overlays, and camera controls.
- technical tooltips on dense script, manual control, overlay, camera, and timeline controls.
- didactic tooltips for robotics concepts such as workspace, TCP, homing, timeline, requested velocity, and effective velocity.
- `Validate` to parse the current DSL script and check Cartesian limits.
- `Simulate` to regenerate the visual playback from the current DSL script.
- validation messages summarize syntax errors, physical limit errors, and invalid command arguments with suggested next steps.
- manual `HOME`, `X+`, `X-`, `Y+`, `Y-`, `Z+`, and `Z-` controls.
- step size and requested speed fields for manual jog commands.
- manual actions append DSL commands and regenerate playback.
- command console for executing one DSL command at a time.
- command history with accepted and rejected command entries.
- `Play` and `Reset` for playback.
- `Prev` and `Next` for frame-by-frame inspection.
- playback speed selector with `0.5x`, `1x`, `2x`, and `4x`.
- Timeline slider for frame scrubbing.
- clickable timeline marker lists for command starts and state changes.
- position chart plotting X/Y/Z over simulated time with a cursor for the current frame.
- effective velocity chart derived from playback samples.
- state chart showing robot state intervals over simulated time.
- requested-versus-effective velocity chart comparing command input with playback behavior.
- accumulated distance chart showing TCP path length over simulated time.
- draggable splitter between the 3D viewport and the side control panel.
- current script line indicator during playback.
- movement explanation panel describing current command behavior.
- MOVE explanations include involved axes, distance, duration, requested speed, effective speed, and axis speed limits.
- overlay toggles for grid, global axes, X/Y/Z labels, workspace bounds, planned path, start/end markers, rails, carriages, and TCP/tool visibility.
- azimuth, elevation, and zoom sliders for camera control.
- mouse drag inside the 3D viewport for orbit rotation.
- `Ctrl+mouse wheel` zoom for active 2D and 3D viewers.
- isometric, front, side, top, and reset camera buttons.
- state panel showing current state, position, command, source line, simulated time, and frame number.
- local example selector and `Load Example` controls for every available desktop viewer.
- `Load Script` and `Save Script` controls for desktop script editors.
- keyboard shortcuts for active viewer script loading, saving, validation, simulation, playback, frame stepping, zoom, and camera reset.
- movement explanation text for SCARA and Simple Articulated Arm joint-space commands.

Print the built-in example script:

```bash
dotnet run --project src/RobotStudio.Cli -- example
```

Validate a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian.robot
```

Simulate a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian.robot
```

Print fixed-interval playback frames for a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- playback examples/cartesian.robot 500
```

Export fixed-interval playback data as JSON:

```bash
dotnet run --project src/RobotStudio.Cli -- export-playback examples/cartesian.robot 500 playback.json
```

Validate exported playback data:

```bash
dotnet run --project src/RobotStudio.Cli -- validate-playback playback.json
```

Current output includes:

- robot profile limits;
- axis velocity and acceleration limits;
- command sequence summary;
- simulation timeline with command source line numbers;
- fixed-interval playback frames when using the `playback` command;
- Cartesian workspace bounds when using the `playback` command;
- JSON playback snapshots when using the `export-playback` command;
- playback snapshot validation when using the `validate-playback` command;
- Cartesian robot poses in exported playback snapshots;
- Cartesian scene frames with renderable primitives in exported playback snapshots;
- Cartesian viewport data for initial 3D camera framing in exported playback snapshots;
- versioned playback metadata in exported playback snapshots;
- final robot state;
- final robot position;
- total simulated duration.

## Current Simulation Capabilities

The simulation library can already execute command sequences created in code.

Supported commands:

- `HomeCommand`
- `MoveToCommand`
- `WaitCommand`

Current simulation output includes:

- initial context;
- final context;
- timeline steps;
- fixed-interval visual playback frames;
- exportable playback snapshots for future visual tools;
- didactic Cartesian mechanism poses derived from playback frames;
- renderable Cartesian scene primitives for future 3D tools;
- deterministic initial viewport data for future 3D tools;
- versioned snapshot metadata for future UI compatibility checks;
- snapshot validation for future UI compatibility checks;
- command source metadata for script-generated steps;
- success/failure flag;
- failure exception when execution cannot continue.

## Current DSL

The simple DSL parser can convert text scripts into command sequences. Internally, scripting now uses a dialect contract so future formats such as G-code can produce the same command sequence type. The CLI can validate and simulate script files.

Cartesian movement:

```txt
MOVE X=120 Y=80 Z=40 SPEED=90
```

Differential drive movement:

```txt
DRIVE X=160 Y=80 HEADING=45 LIN=120 ANG=90
```

`LIN` is requested linear velocity in millimeters per second. `ANG` is requested angular velocity in degrees per second.

Drone movement:

```txt
DRONE X=120 Y=80 Z=40 YAW=90 SPEED=100 YAW_SPEED=45
```

`SPEED` is requested 3D linear velocity in millimeters per second. `YAW_SPEED` is requested yaw velocity in degrees per second.

Six-joint industrial arm movement:

```txt
ARM6 J1=45 J2=30 J3=-20 J4=90 J5=15 J6=180 SPEED=80
```

`J1` through `J6` are target joint angles in degrees. `SPEED` requests a coordinated joint velocity in degrees per second, capped by the slowest involved joint.

```txt
HOME
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
```

Current parser behavior:

- `HOME` moves the first Cartesian robot to `(0, 0, 0)`;
- `MOVE` moves to a Cartesian position;
- `DRONE` moves to a simplified aerial pose in the core simulator;
- `ARM6` coordinates six industrial-arm joints in the core simulator;
- `WAIT` advances simulated time without moving the robot;
- `SPEED` requests a movement speed in millimeters per second;
- physical axis limits still cap the effective movement speed;
- parser errors include the script line number.
- G-code is tracked as a planned future dialect, not as an executable parser.

## Planned CLI Learning Flow

The CLI should later support:

- richer help output;
- more examples;
- friendlier formatting for script validation errors.

## Not Available Yet

- G-code parsing.
- Hardware communication.
- Arduino or ESP32 integration.
