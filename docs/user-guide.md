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

## Run Tests

Run from the repository root:

```bash
dotnet test
```

Build a Windows CLI release artifact:

```bash
powershell -ExecutionPolicy Bypass -File scripts/release/build-cli-artifact.ps1 -Version 1.1.0 -Runtime win-x64
```

The supported CLI release runtime is:

- `win-x64`

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

The Cartesian catalog card also opens a separate mechanical showcase through `Explore Mechanics`. In this view, drag with the left mouse button to orbit, drag with the middle mouse button or `Shift` + left mouse button to pan, and use `Ctrl` + mouse wheel to zoom. `Reset` restores the demonstration and a camera framing calculated from the packaged model. The `View layer` selector switches among the assembled machine, the transparent drive-system inspection layer, and a motion-axis layer with red X, green Y, and blue Z direction guides without replacing the schematic simulator. The demonstration selector offers a coordinated practical tour and an individual-axis inspection; the description beneath it explains the active sequence before playback.

Opening the XY plotter renders a beginner two-axis drawing model on a fixed `Z=0` drawing plane. It uses X/Y movement while reusing the same script validation, playback, timeline, chart, overlay controls, and local example selector.

Opening the differential drive robot renders a 2D mobile robot viewer with workspace grid, playback path, robot body, wheels, heading indicator, current pose, command name, and timeline controls. Translation and rotation execute sequentially with independent velocity and acceleration limits. The movement explanation shows ideal accumulated travel and rotation for the left and right wheels. This odometry is deterministic and does not yet model encoder noise or wheel slip. The viewer includes a mobile DSL editor for `HOME`, `DRIVE`, and `WAIT` commands, plus a local example selector.

Opening the SCARA robot renders a 3D articulated robot viewer with reachable workspace, volumetric base, shoulder joint, elbow joint, tool point, planned path, current joint angles, current tool pose, command name, camera orbit, zoom, and timeline controls. Its coordinated joint playback respects angular velocity and acceleration limits. The viewer includes a SCARA DSL editor for `HOME`, `SCARA`, and `WAIT` commands, plus a local example selector.

Opening the Simple Articulated Arm renders a 3D three-joint arm viewer with reachable workspace, volumetric base, base joint, shoulder, elbow, tool point, tool orientation, planned path, current joint angles, current tool pose, command name, camera orbit, zoom, and timeline controls. Its coordinated playback respects angular velocity and acceleration limits. The viewer switches between Simple DSL `HOME`/`ARM`/`WAIT` joint-space lessons and G-code `G1 X/Y/A` tool-pose lessons, with local examples for both dialects.

Opening the Delta Robot renders a 3D simplified parallel robot viewer with a triangular frame, three vertical actuators, moving carriages, parallel links, platform/TCP, reachable workspace, planned path, current actuator positions, current tool pose, command name, camera orbit, zoom, and timeline controls. Its three linear actuators move with one synchronized acceleration-aware profile. The viewer switches between Simple DSL `HOME`/`DELTA`/`WAIT` actuator-space lessons and G-code `G1 X/Y/Z` tool-space lessons, with local examples for both dialects.

Opening the Drone renders a 3D aerial robot viewer with flight-volume boundaries, ground grid, drone body, rotor arms, attitude and heading indicators, planned path, current X/Y/Z position, current roll/pitch/yaw, command name, camera orbit, zoom, and timeline controls. Translation, roll/pitch attitude, and yaw use independent but time-synchronized acceleration-aware profiles. This remains a simplified deterministic teaching model rather than an aerodynamic or flight-control simulation. The viewer includes a Drone DSL editor for `HOME`, `DRONE`, and `WAIT` commands, plus a local example selector.

Opening the 6-DOF Industrial Arm renders a 3D serial arm viewer with a raised base, six joint markers, volumetric links, wrist/tool orientation, reachable floor area, TCP path, joint state, command name, camera orbit, zoom, and timeline controls. Its dialect selector switches between Simple DSL `HOME`/`ARM6`/`WAIT` joint-space lessons and G-code `G1 X/Y/Z/A/B/C` tool-pose lessons, with local examples for both.

Every available desktop viewer includes an example selector and a `Load Example` button. The non-Cartesian side panels also explain current movement concepts where that viewer already has a didactic explanation panel.

Simulation workspaces use the same visual hierarchy across robot families: the active robot is identified in the header, the simulation viewport remains the primary surface, state/script/explanation information is grouped in the side panel, and playback navigation remains in the timeline footer. `Play` and `Simulate` are emphasized as primary actions; navigation, validation, file operations, and reset actions use a quieter secondary treatment.

Script editors in the desktop app can load and save local `.robot` or `.txt` files. Loading a script replaces the editor text and asks the student to validate or simulate before playback. Saving writes the current editor text without changing the simulation.

When validation fails, the desktop app shows a student-facing summary. Syntax errors include the script line number when available, physical limit errors explain that the target is outside the workspace, and command argument errors suggest checking required values such as speed or duration.

Desktop keyboard shortcuts:

- `Ctrl+G`: open or close the searchable robotics glossary from the catalog or any simulation workspace.
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
powershell -ExecutionPolicy Bypass -File scripts/release/build-windows-installer.ps1 -Version 1.1.0 -Runtime win-x64
```

The installer is generated at:

```txt
artifacts/release/RobotStudio-1.1.0-win-x64-setup.exe
```

The script also generates a SHA256 checksum file next to the installer:

```txt
artifacts/release/RobotStudio-1.1.0-win-x64-setup.exe.sha256
```

For official releases, push a version tag such as `v1.1.0`. GitHub Actions derives the artifact version from the tag and publishes a GitHub Release with the Windows installer, Windows CLI ZIP archive, and SHA256 checksum files attached.

Current desktop controls:

- searchable robotics glossary with topic filters, available from the catalog and every simulation workspace.
- robot cards showing name, family, compact status and complexity tags, description, and capability tags.
- robot selection cards arrange responsively across one to six columns according to available width and a comfortable target card size.
- available robot selection cards can be opened from the card body, the `Open Robot` button, or the keyboard and show hover and focus feedback.
- non-interactive planned-release labels on future robot entries ordered by didactic complexity.
- contextual `HOME` and `Reset Fault` actions inside the script panel. They remain hidden until the retained simulation context becomes faulted.
- `Robots` inside the Cartesian viewer to return to the selection screen.
- Simple DSL/G-code dialect selector and script editor inside the Cartesian viewer.
- script editor gutter with line numbers and command tags for `HOME`, `MOVE`, and `WAIT`.
- collapsible sidebar panels for script, manual control, command console, robot state, charts, movement explanation, timeline markers, overlays, and camera controls.
- technical tooltips on dense script, manual control, overlay, camera, and timeline controls.
- didactic tooltips for robotics concepts such as workspace, TCP, homing, timeline, requested velocity, and effective velocity.
- `Validate` to parse the current script dialect and check Cartesian limits.
- `Simulate` to regenerate visual playback from the current script.
- validation messages summarize syntax errors, physical limit errors, and invalid command arguments with suggested next steps.
- manual `HOME`, `X+`, `X-`, `Y+`, `Y-`, `Z+`, and `Z-` controls.
- step size and requested speed fields for manual jog commands.
- manual actions append commands in the selected dialect and regenerate playback.
- command console for executing one command in the selected dialect at a time.
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
- Cartesian robot configuration for axis minimum, maximum, maximum velocity, and maximum acceleration.
- `Load Script` and `Save Script` controls for desktop script editors.
- keyboard shortcuts for active viewer script loading, saving, validation, simulation, playback, frame stepping, zoom, and camera reset.
- movement explanation text for SCARA and Simple Articulated Arm joint-space commands.

### Cartesian Robot Configuration

The Cartesian workspace includes a collapsible `Robot Configuration` panel. Each X/Y/Z row accepts minimum and maximum position in millimeters, maximum velocity in millimeters per second, and maximum acceleration in millimeters per second squared.

`Apply` validates the values through the domain `Axis` and `CartesianRobotProfile` types, resets the simulation origin to HOME, rebuilds the workspace, and regenerates the current script. The configured range must include `(0, 0, 0)` because HOME is fixed at the Cartesian origin.

If the profile is valid but the current script exceeds its new limits, the profile remains active, the script error is shown, and the viewport displays a safe HOME preview. `Restore` reapplies the default teaching profile. Configuration remains in memory for the current Cartesian workspace session; profile files are not persisted yet.

Print the built-in example script:

```bash
dotnet run --project src/RobotStudio.Cli -- example
```

Validate a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian/basic.robot
```

Simulate a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian/basic.robot
```

Validate or simulate the equivalent G-code example:

```bash
dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian/basic.gcode
dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian/basic.gcode
```

The CLI infers the dialect from `.robot` and `.gcode`. Use an explicit override for neutral file extensions or classroom comparisons:

```bash
dotnet run --project src/RobotStudio.Cli -- simulate lesson.txt --dialect gcode
dotnet run --project src/RobotStudio.Cli -- example --dialect gcode
```

The explicit option takes precedence over the extension and accepts `dsl` or `gcode`.

Print fixed-interval playback frames for a script file:

```bash
dotnet run --project src/RobotStudio.Cli -- playback examples/cartesian/basic.robot 500
```

Export fixed-interval playback data as JSON:

```bash
dotnet run --project src/RobotStudio.Cli -- export-playback examples/cartesian/basic.robot 500 playback.json
```

Validate exported playback data:

```bash
dotnet run --project src/RobotStudio.Cli -- validate-playback playback.json
```

Current output includes:

- selected script dialect for validation, simulation, and playback flows;
- robot profile limits;
- axis velocity and acceleration limits;
- command sequence summary;
- simulation timeline with command source line numbers;
- fixed-interval playback frames when using the `playback` command;
- exact Cartesian motion-profile phase, velocity, and acceleration in version 2 playback frames;
- requested Cartesian movement velocity and wait duration in version 3 playback frames;
- exact Cartesian command motion summaries in version 4, including triangular or trapezoidal shape, involved axes, limiting velocity, peak velocity, acceleration, and phase durations;
- Cartesian workspace bounds when using the `playback` command;
- JSON playback snapshots when using the `export-playback` command;
- playback snapshot validation when using the `validate-playback` command;
- Cartesian robot poses in exported playback snapshots;
- Cartesian scene frames with renderable primitives in exported playback snapshots;
- Cartesian viewport data for initial 3D camera framing in exported playback snapshots;
- versioned playback metadata in exported playback snapshots;
- compatibility validation for Cartesian snapshot formats 1, 2, 3, and 4;
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

## Current Script Dialects

The Simple DSL parser converts text scripts into command sequences for every implemented robot family. Cartesian, XY Plotter, SCARA, Simple Articulated Arm, Delta, and 6-DOF Industrial Arm workspaces also provide an introductory G-code dialect selector. Both dialects produce domain commands before physical validation and simulation.

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
DRONE X=120 Y=80 Z=40 ROLL=10 PITCH=-5 YAW=90 SPEED=100 ATTITUDE_SPEED=60 YAW_SPEED=45
```

`SPEED` is requested 3D linear velocity in millimeters per second. `ATTITUDE_SPEED` is the requested shared roll/pitch velocity in degrees per second. `YAW_SPEED` is requested yaw velocity in degrees per second. `ROLL`, `PITCH`, and `ATTITUDE_SPEED` are optional; omitted roll and pitch values default to zero for compatibility with earlier scripts.

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
- `RESET` acknowledges a fault and returns a resumed simulation context to `Idle` without moving the robot or changing simulated time;
- `MOVE` moves to a Cartesian position;
- `DRONE` moves to a simplified aerial pose in the core simulator;
- `ARM6` coordinates six industrial-arm joints in the core simulator;
- `WAIT` advances simulated time without moving the robot;
- `SPEED` requests a movement speed in millimeters per second;
- physical axis limits still cap the effective movement speed;
- parser errors include the script line number.

Introductory Cartesian G-code:

```gcode
G21
G90
G28
G1 X120 Y80 Z40 F5400
G91
G1 X20 Y-10
G4 P500
```

- `G21` explicitly selects millimeters; `G20` inch mode is rejected with a corrective message.
- `G28` maps to homing.
- `G90` selects absolute positioning and is the default mode.
- `G91` selects relative positioning.
- `G1` requires at least one coordinate. In absolute mode omitted axes retain their current position; in relative mode they do not move.
- `F` is an optional feed rate in millimeters per minute; RobotStudio converts it to millimeters per second for the shared movement command.
- `G4 P` maps to a dwell duration in milliseconds.
- `N` line numbers, compact words, semicolon comments, and parenthesized comments are accepted.
- Loading a `.gcode` or `.robot` file selects its matching dialect automatically; `.txt` keeps the current selection.
- `G90` and `G91` do not create timeline movements themselves; they control how subsequent `G1` lines are resolved.
- The generated G-code preamble uses `G21` and `G90` so units and positioning are explicit.
- Select `Explain G-code lines` below a G-code editor to show or hide explanations for each supported line. The guide follows script edits and distinguishes robot-specific coordinates without changing the program.
- G-code coordinates describe TCP tool-space motion and never stand for joint numbers or actuators.
- Cartesian Robot and XY Plotter mappings are available through direct linear axes.
- SCARA accepts planar `G1 X/Y` and optional `Z0`. It follows a sampled linear TCP path using deterministic elbow-down inverse kinematics; use `HOME`/`G28` or an elbow-down pose before tool-space movement.
- Simple Articulated Arm accepts planar `G1 X/Y/A` and optional `Z0`. `A` controls TCP orientation in degrees, while `B/C` are rejected. Use `HOME`/`G28` or a positive-bend pose before tool-space movement.
- Delta Robot accepts `G1 X/Y/Z`. Exact inverse kinematics converts each TCP target into synchronized actuator positions; `A/B/C` orientation words are rejected by the current position-only model.
- 6-DOF Industrial Arm accepts `G1 X/Y/Z/A/B/C`, where `A/B/C` are roll/pitch/yaw in degrees. It uses deterministic positive-elbow/wrist-neutral inverse kinematics; the introductory topology requires yaw `C` to match the azimuth of `X/Y` and rejects incompatible poses.
- Differential Drive and Drone use their robot-appropriate Simple DSL commands instead of a nonstandard G-code mapping.
- Hardware execution and other G-codes are not part of this subset.

## Teaching Examples

Standalone scripts are grouped by robot model under `examples/`. The Cartesian desktop selector includes these focused lessons in both Simple DSL and G-code:

- `Axis limit validation (invalid)` intentionally exceeds the X-axis maximum and should be validated to inspect the error message;
- `Requested vs effective speed` isolates X, Y, and Z movements so axis-specific speed caps are visible in the charts;
- `Jog, wait, and home sequence` mirrors small manual jog actions, pauses without movement, and returns to the origin.

An intentional validation failure is a teaching asset, not an executable success example. Its catalog metadata and automated tests record that expectation explicitly.

The SCARA, Simple Articulated Arm, and Delta workspaces offer Simple DSL and G-code examples. Simple DSL commands teach direct joint or actuator movement. Their G-code examples teach TCP tool-space movement through inverse kinematics, including planar tool orientation with `A` for the Simple Arm and synchronized parallel-actuator movement for Delta. The `G-code` capability badge appears only on robot cards with an executable mapping.

The desktop app preserves the latest typed simulation context for the active robot. `Reset Fault` starts a recovery playback from a faulted context without changing pose, joints, actuator state, odometry, attitude, or elapsed time. `HOME` starts a physical homing playback from the same preserved context and remains available from every state.

The Cartesian simulation API can also receive a `CartesianSimulationEnvironment` containing axis-aligned obstacle volumes. Cartesian `MOVE` and `HOME` commands reject paths that touch or cross an obstacle and report the obstacle ID and first collision position. The current desktop app does not yet provide obstacle editing or visualization.

The Differential Drive API accepts a `PlanarSimulationEnvironment` and uses the profile's explicit circular collision radius. This means collision checks include the robot body, not only its center point. Blocked `DRIVE` and `HOME` commands preserve pose, odometry, and simulated time. Planar obstacle controls and overlays are not yet exposed in the desktop viewer.

The SCARA API reuses planar obstacles while modeling each robot link as a capsule with an explicit profile radius. Joint movements and homing inspect deterministic intermediate configurations, identifying whether the first or second link would collide. The default maximum joint-space sampling step is one degree. SCARA obstacle controls and collision overlays are not yet exposed in the desktop viewer.

The Simple Articulated Arm, 6-DOF Industrial Arm, Delta Robot, and Drone accept `SpatialSimulationEnvironment`. Arm profiles define link radii, Delta defines a moving-component radius, and Drone defines a body radius. Blocked commands identify the affected semantic component and preserve the prior simulation state. These are deterministic introductory safety envelopes; mesh collision, self-collision, and desktop obstacle authoring remain future work.

## Planned CLI Learning Flow

The CLI should later support:

- richer help output;
- more examples;
- friendlier formatting for script validation errors.

## Not Available Yet

- Extended or hardware-executed G-code.
- Hardware communication.
- Arduino or ESP32 integration.
