# Test Map

This document maps expected automated tests to project behavior. It should be updated whenever new behavior is added.

## `RobotStudio.Domain.Tests`

### Current Coverage

- [x] Valid Cartesian position inside limits does not throw.
- [x] Cartesian position outside limits throws `PositionOutOfRangeException`.
- [x] Full robot profile validates positions exactly at minimum X/Y/Z limits.
- [x] Full robot profile validates positions exactly at maximum X/Y/Z limits.
- [x] Cartesian position implements the general robot position contract.
- [x] Cartesian robot profile implements the general robot profile contract.
- [x] Axis configuration rejects invalid limits.
- [x] Axis configuration rejects non-positive maximum velocity.
- [x] Axis configuration rejects non-positive maximum acceleration.
- [x] Axis accepts coordinates exactly at minimum and maximum limits.
- [x] Robot state exposes the first execution state values.
- [x] Robot state initial value is explicitly defined as `Idle`.
- [x] `HOME` can transition from every robot state to `Homing`.
- [x] Ready states can transition to `Moving`.
- [x] Ready states can transition to `Waiting`.
- [x] Active states can transition to `Completed`.
- [x] Non-faulted states can transition to `Faulted`.
- [x] State helpers identify active states.
- [x] State helpers identify states ready for normal commands.
- [x] State helpers identify command end states.
- [x] State helpers identify recoverable states.
- [x] Invalid state transitions return `false`.
- [x] Invalid enforced state transitions throw `InvalidRobotStateTransitionException`.
- [x] Command validator accepts `HOME`.
- [x] Command validator accepts `WAIT`.
- [x] Command validator validates `MOVE` target position.
- [x] XY plotter profile validates X/Y positions.
- [x] XY plotter profile rejects Z axis access.
- [x] Command validator rejects XY plotter movement away from the `Z=0` drawing plane.
- [x] Differential drive profile validates planar poses.
- [x] Differential drive profile rejects poses outside the X/Y workspace.
- [x] Differential drive pose normalizes headings for one turn.
- [x] Differential drive pose computes shortest angular distance.
- [x] SCARA profile validates joint positions.
- [x] SCARA profile rejects joint positions outside limits.
- [x] SCARA forward kinematics calculates tool pose.
- [x] SCARA inverse kinematics calculates reachable elbow-down joint positions.
- [x] SCARA inverse kinematics rejects unreachable tool poses.
- [x] Simple arm profile validates joint positions.
- [x] Simple arm profile rejects joint positions outside limits.
- [x] Simple arm forward kinematics calculates tool pose and orientation.
- [x] Command validator validates simple arm joint move commands.
- [x] Industrial arm profile validates all six joint coordinates and identifies out-of-limit joints.
- [x] Industrial arm profile rejects missing or duplicated joint definitions.
- [x] Industrial arm joint displacement considers all six joints.
- [x] Industrial arm simplified forward kinematics calculates TCP position and orientation.
- [x] Delta profile validates actuator positions.
- [x] Delta profile rejects actuator positions outside limits.
- [x] Delta simplified forward kinematics maps actuator displacement to tool pose.
- [x] Command validator validates Delta actuator move commands.
- [x] Drone profile validates 3D flight poses.
- [x] Drone profile rejects poses outside the X/Y/Z flight volume.
- [x] Drone pose computes shortest yaw rotation.
- [x] Command validator validates Drone move commands.
- [x] `WaitCommand` rejects negative duration with `InvalidRobotCommandException`.
- [x] `MoveToCommand` rejects non-positive requested velocity with `InvalidRobotCommandException`.
- [x] Domain error messages identify invalid values and expected ranges or states.
- [x] `ImpossibleMovementException` exposes the reason a movement cannot be planned.
- [x] Null command sequence input is rejected.
- [x] Empty command sequence is rejected.
- [x] Command sequence containing null command is rejected.
- [x] Valid command sequence preserves command order.
- [x] Command source metadata validates positive line numbers.
- [x] Command source metadata rejects blank source text.

### Required Next Coverage

- [x] `RESET` is accepted only from `Faulted` and returns the logical state to `Idle`.
- [x] Every implemented simulator preserves its family-specific physical state and elapsed time during fault reset.
- [x] Differential Drive fault reset also preserves accumulated ideal odometry.
- [x] Simple DSL `RESET` parsing preserves source metadata and rejects arguments.
- [x] Cartesian segment/AABB collision detection reports the nearest obstacle, entry point, and trajectory fraction.
- [x] Cartesian collision detection handles clear paths, boundary contact, stationary occupied positions, and duplicate obstacle IDs deterministically.
- [x] Cartesian `MOVE` and `HOME` reject obstructed paths while preserving the last valid position.
- [x] Differential Drive collision checks use an explicit finite positive body radius.
- [x] Swept circular-footprint tests cover obstacle sides, rounded-corner tangency, clear corner paths, and initially overlapping poses.
- [x] Differential Drive `DRIVE` and `HOME` reject obstructed paths without changing pose, odometry, or elapsed time.
- [x] SCARA collision tests identify first-link and second-link occupancy independently.
- [x] SCARA joint-path sampling detects intermediate collisions while accepting clear articulated paths.
- [x] SCARA move and home commands preserve joints and elapsed time when a link path is obstructed.
- [x] SCARA profiles reject non-finite or non-positive link collision radii.
- [x] Spatial envelope tests cover swept body entry and clear link geometry.
- [x] Simple Articulated Arm and 6-DOF Industrial Arm fixtures reject intermediate link obstructions.
- [x] Delta fixtures reject moving-platform or parallel-link obstructions without changing actuator state.
- [x] Drone fixtures reject body-envelope obstructions without changing pose or attitude.
- [x] Every implemented robot family has deterministic simulator success, failure, recovery, and collision coverage appropriate to its topology.

## `RobotStudio.Motion.Tests`

### Current Coverage

- [x] Planner creates a plan for valid movement.
- [x] Planner implements the general motion planner contract for the Cartesian profile.
- [x] Planner rejects target position outside limits.
- [x] Non-zero displacement has positive duration.
- [x] Start position equal to end position returns a stationary plan.
- [x] Requested velocity below the axis limit is used.
- [x] Requested velocity above the axis limit is capped by the axis limit.
- [x] Motion plan exposes total distance.
- [x] Motion segment exposes involved axes.
- [x] Scalar profile creates trapezoidal motion when the configured velocity can be reached.
- [x] Scalar profile creates triangular motion when the movement is too short to reach the configured velocity.
- [x] Scalar profile samples acceleration, constant-velocity, deceleration, and completed phases deterministically.
- [x] Scalar profile clamps sampling outside its time range and rejects invalid inputs.
- [x] Cartesian planner uses the lowest acceleration limit among involved axes.
- [x] Cartesian acceleration-aware duration exceeds the constant-velocity estimate.
- [x] XY plotter movement exposes an acceleration-aware profile.
- [x] SCARA planning uses the lowest angular acceleration limit among involved joints.
- [x] Simple articulated arm planning exposes an acceleration-aware angular profile.
- [x] 6-DOF industrial arm planning synchronizes all involved joints with one constrained angular profile.
- [x] Differential-drive planning uses independent linear and angular acceleration profiles.
- [x] Differential-drive playback completes translation before rotation and follows each segment profile.
- [x] Differential-drive odometry accumulates equal wheel travel during translation.
- [x] Differential-drive odometry produces opposite signed wheel travel during in-place rotation.
- [x] Differential-drive playback exposes intermediate acceleration-aware wheel odometry.
- [x] Delta planning and playback synchronize involved actuators with the lowest acceleration limit.
- [x] Drone planning and playback synchronize independent translation, attitude, and yaw profiles without mixing units.
- [x] Drone profiles validate physical roll and pitch limits.
- [x] Drone planning synchronizes roll/pitch attitude with translation and yaw.
- [x] Drone playback interpolates roll, pitch, and yaw using acceleration-aware progress.
- [x] Planner rejects impossible movement when distance exists but no axis displacement is measurable.
- [x] XY plotter planner creates valid X/Y motion plans.
- [x] XY plotter planner rejects target positions outside X/Y limits.
- [x] XY plotter planner handles stationary movement predictably.
- [x] Differential drive planner creates translation plans.
- [x] Differential drive planner creates rotation plans.
- [x] Differential drive planner separates translation and rotation when both are required.
- [x] Differential drive planner caps requested linear and angular velocities.
- [x] SCARA planner creates coordinated joint-space plans.
- [x] SCARA planner caps requested joint velocity by involved joint limits.
- [x] SCARA planner handles stationary joint movement predictably.
- [x] SCARA planner rejects target joints outside physical limits.
- [x] Simple arm planner creates coordinated joint-space plans.
- [x] Simple arm planner caps requested joint velocity by involved joint limits.
- [x] Simple arm planner handles stationary joint movement predictably.
- [x] Simple arm planner rejects target joints outside physical limits.
- [x] Industrial arm planner coordinates all involved joints in one segment.
- [x] Industrial arm planner uses the slowest involved joint limit and handles stationary movement.
- [x] Industrial arm planner reports only the joints involved in a partial wrist movement.
- [x] Industrial arm planner rejects non-positive requested joint velocity.
- [x] Industrial arm planner rejects target joints outside physical limits.
- [x] Delta planner creates coordinated actuator-space plans.
- [x] Delta planner caps requested actuator velocity by involved actuator limits.
- [x] Delta planner handles stationary actuator movement predictably.
- [x] Delta planner rejects target actuators outside physical limits.
- [x] Drone planner creates coordinated 3D translation, roll/pitch attitude, and yaw plans.
- [x] Drone planner caps requested linear, attitude, and yaw velocities.
- [x] Drone planner handles stationary pose movement predictably.
- [x] Drone planner rejects target poses outside physical limits.

### Required Next Coverage

- [x] Add family-specific acceleration profile tests for every currently available robot family.

## `RobotStudio.Simulation.Tests`

### Current Coverage

- [x] New simulation context starts in `Idle`.
- [x] `HOME` moves the robot to origin and ends in `Completed`.
- [x] `MOVE` updates final position and ends in `Completed`.
- [x] `MOVE` with requested velocity includes acceleration and deceleration in its duration.
- [x] Zero-distance `MOVE` completes without advancing simulated time.
- [x] `WAIT` advances simulated time without moving.
- [x] A sequence containing `HOME`, `MOVE`, and `WAIT` executes in order.
- [x] Failing command sequence ends in `Faulted`.
- [x] Failing command sequence preserves the last valid position.
- [x] Invalid initial simulation context is rejected.
- [x] Timeline command steps preserve zero-based command index.
- [x] Timeline command steps preserve command name.
- [x] Timeline simulator steps have no command source.
- [x] Failing command timeline step preserves command source.
- [x] Timeline command steps preserve command source metadata.
- [x] Timeline sampling preserves command source metadata.
- [x] Timeline sampling before the first step returns the initial position.
- [x] Timeline sampling during movement returns an interpolated position.
- [x] Timeline sampling during Cartesian movement follows acceleration-aware profile progress.
- [x] SCARA, Simple Articulated Arm, and 6-DOF Industrial Arm playback follows acceleration-aware angular progress.
- [x] Timeline sampling exposes exact profile phase, velocity, and acceleration.
- [x] A completed movement exposes zero velocity, zero acceleration, and the completed phase.
- [x] Timeline sampling during wait keeps the same position.
- [x] Timeline sampling after the final step returns the final position.
- [x] Cartesian visual-state mapping preserves position in millimeters.
- [x] Cartesian visual-state mapping preserves state and command metadata.
- [x] Cartesian visual-state mapping preserves motion-profile metrics.
- [x] Cartesian visual-state mapping rejects null samples.
- [x] Cartesian visual-state sampling returns visual state for sampled simulation time.
- [x] Cartesian visual-state sampling preserves command source metadata.
- [x] Cartesian visual-state sampler rejects null dependencies.
- [x] Cartesian playback sampling returns frames at fixed intervals.
- [x] Cartesian playback sampling always includes the final frame.
- [x] Cartesian playback sampling avoids duplicate final frames.
- [x] Cartesian playback sampling handles zero-duration simulations.
- [x] Cartesian playback sampling rejects non-positive intervals.
- [x] Cartesian workspace bounds use robot axis limits.
- [x] Cartesian workspace bounds expose workspace size and center.
- [x] Cartesian workspace bounds identify positions inside, on, and outside the workspace.
- [x] Cartesian playback snapshot includes workspace bounds, frames, duration, and success state.
- [x] Cartesian playback snapshot preserves failure messages.
- [x] Cartesian robot pose mapping creates base, X carriage, Y carriage, Z carriage, and tool center point positions.
- [x] Cartesian robot pose mapping preserves timeline metadata.
- [x] Cartesian scene frame mapping creates renderable workspace, rail, carriage, and tool primitives.
- [x] Cartesian scene frame mapping positions moving primitives from robot poses.
- [x] Cartesian scene frame mapping preserves pose metadata.
- [x] Cartesian viewport planning targets the workspace center.
- [x] Cartesian viewport planning creates a deterministic diagonal camera position.
- [x] Cartesian viewport planning creates positive clipping distances.
- [x] Playback snapshot metadata exposes format version, robot family, units, and sample interval.
- [x] Playback snapshot metadata rejects non-positive sample intervals.
- [x] Playback snapshot validation accepts compatible snapshots.
- [x] Playback snapshot validation reports incompatible metadata.
- [x] Playback snapshot validation reports missing sections and inconsistent counts.
- [x] Playback snapshot version 2 validates frame velocity and acceleration values.
- [x] Playback snapshot validation remains compatible with version 1 metadata.
- [x] Version 1 frames without motion metrics deserialize with compatible defaults.
- [x] Differential drive simulator executes `HOME`.
- [x] Differential drive simulator executes differential-drive move commands.
- [x] Differential drive simulator executes `WAIT`.
- [x] Differential drive simulator records state transitions in order.
- [x] Differential drive simulator returns `Faulted` when a command fails.
- [x] Differential drive simulator preserves the last valid pose when a command fails.
- [x] Differential drive playback sampling returns frames at fixed intervals.
- [x] Differential drive playback sampling interpolates pose between timeline steps.
- [x] Differential drive playback sampling preserves command metadata.
- [x] SCARA simulator executes `HOME`.
- [x] SCARA simulator executes joint move commands.
- [x] SCARA simulator executes `WAIT`.
- [x] SCARA simulator records state transitions in order.
- [x] SCARA simulator returns `Faulted` when a command fails.
- [x] SCARA playback sampling returns frames at fixed intervals.
- [x] SCARA playback sampling interpolates joint position between timeline steps.
- [x] SCARA playback sampling preserves command metadata.
- [x] SCARA playback sampling includes tool poses calculated from kinematics.
- [x] Simple arm simulator executes `HOME`.
- [x] Simple arm simulator executes joint move commands.
- [x] Simple arm simulator executes `WAIT`.
- [x] Simple arm simulator returns `Faulted` when a command fails.
- [x] Simple arm playback sampling returns frames at fixed intervals.
- [x] Simple arm playback sampling interpolates joint position between timeline steps.
- [x] Simple arm playback sampling preserves command metadata.
- [x] Simple arm playback sampling includes tool poses calculated from kinematics.
- [x] Industrial arm simulator executes six-joint moves, `WAIT`, and `HOME` in sequence.
- [x] Industrial arm simulator faults on an invalid command and preserves the last valid joint position.
- [x] Industrial arm playback sampling interpolates all six joints and preserves command metadata.
- [x] Industrial arm playback sampling rejects non-positive sample intervals.
- [x] Industrial arm playback frames include TCP poses calculated from forward kinematics.
- [x] Delta simulator executes `HOME`.
- [x] Delta simulator executes actuator move commands.
- [x] Delta simulator returns `Faulted` when a command fails.
- [x] Delta playback sampling returns frames at fixed intervals.
- [x] Delta playback sampling interpolates actuator position between timeline steps.
- [x] Delta playback sampling preserves command metadata.
- [x] Drone simulator executes `HOME`.
- [x] Drone simulator executes 3D pose move commands.
- [x] Drone simulator returns `Faulted` when a command fails.
- [x] Drone playback sampling returns frames at fixed intervals.
- [x] Drone playback sampling interpolates position and full attitude between timeline steps.
- [x] Drone playback sampling preserves command metadata.
- [x] Cartesian, mobile, SCARA, simple arm, industrial arm, Delta, and Drone snapshots expose the shared playback snapshot contract.
- [x] Cartesian, mobile, SCARA, simple arm, industrial arm, Delta, and Drone frames expose common timeline metadata.
- [x] Shared playback summaries can summarize snapshots without knowing the robot family's concrete position type.

### Required Next Coverage

- [ ] Add richer failure recovery tests after the recovery workflow is defined.

## `RobotStudio.Scripting.Tests`

### Current Coverage

- [x] The Simple DSL parser implements the script dialect contract.
- [x] The Simple DSL dialect is marked as available.
- [x] The G-code parser implements the script dialect contract and is marked as available.
- [x] Parse `HOME`.
- [x] Parse `WAIT 500`.
- [x] Parse `MOVE X=10 Y=20 Z=5`.
- [x] Parse `MOVE X=10 Y=20 Z=5 SPEED=100`.
- [x] Parse `DRIVE X=10 Y=20 HEADING=90`.
- [x] Parse `DRIVE X=10 Y=20 HEADING=90 LIN=100 ANG=45`.
- [x] Parse `SCARA SHOULDER=45 ELBOW=30`.
- [x] Parse `SCARA SHOULDER=45 ELBOW=30 SPEED=80`.
- [x] Parse `ARM BASE=45 SHOULDER=30 ELBOW=-20`.
- [x] Parse `ARM BASE=45 SHOULDER=30 ELBOW=-20 SPEED=80`.
- [x] Parse `ARM6 J1=45 J2=30 J3=-20 J4=90 J5=15 J6=180 SPEED=80`.
- [x] Missing industrial arm joint argument reports a clear parser error.
- [x] Parse `DELTA A=30 B=60 C=90`.
- [x] Parse `DELTA A=30 B=60 C=90 SPEED=80`.
- [x] Parse `DRONE X=120 Y=80 Z=40 YAW=90`.
- [x] Parse `DRONE X=120 Y=80 Z=40 YAW=90 SPEED=100 YAW_SPEED=45`.
- [x] Parse optional Drone `ROLL`, `PITCH`, and `ATTITUDE_SPEED` arguments while keeping older scripts compatible.
- [x] Unknown command reports a clear parser error.
- [x] Missing coordinate reports a clear parser error.
- [x] Missing `DRIVE` heading reports a clear parser error.
- [x] Invalid number reports a clear parser error.
- [x] Invalid wait duration reports a clear parser error.
- [x] Parser errors preserve script line number.
- [x] Parsed commands preserve script line number metadata.
- [x] Parsed commands preserve script text metadata.
- [x] Duplicate MOVE argument reports a clear parser error.
- [x] Unknown MOVE argument reports a clear parser error.
- [x] Unknown DRIVE argument reports a clear parser error.
- [x] Missing SCARA joint argument reports a clear parser error.
- [x] Unknown SCARA argument reports a clear parser error.
- [x] Missing simple arm joint argument reports a clear parser error.
- [x] Unknown simple arm argument reports a clear parser error.
- [x] Missing Delta actuator argument reports a clear parser error.
- [x] Unknown Delta argument reports a clear parser error.
- [x] Missing Drone yaw argument reports a clear parser error.
- [x] Unknown Drone argument reports a clear parser error.
- [x] HOME with arguments reports a clear parser error.

### Required Next Coverage

- [x] Parse `G28`, `G1 X... Y... Z... F...`, and `G4 P...` into shared domain commands.
- [x] Convert G-code feed rate from millimeters per minute to millimeters per second.
- [x] Preserve G-code source line metadata and comments.
- [x] Accept optional `N` line numbers and compact G-code words.
- [x] Reject missing coordinates, duplicate words, invalid feed rates, and unsupported codes with line-aware errors.
- [x] Write supported domain command sequences as equivalent G-code.
- [x] Parse `G90` absolute positioning while retaining omitted coordinates from the supplied parse context.
- [x] Parse consecutive `G91` relative movements into accumulated absolute targets.
- [x] Switch from `G91` back to `G90` predictably.
- [x] Treat `G28` as a known origin for later relative movement.
- [x] Reject relative movement without a known starting position.
- [x] Reject arguments on `G90` and `G91`.
- [x] Classify `G90` and `G91` as positioning-mode lines in the desktop script gutter.
- [x] Validate dedicated Cartesian G-code examples from the viewer's initial position.

## `RobotStudio.Hardware.Tests`

### Current Coverage

- [x] Hardware command envelopes preserve command id, command, and timeout.
- [x] Hardware command envelopes reject empty command ids.
- [x] Hardware command envelopes reject null commands.
- [x] Hardware command envelopes reject non-positive timeouts.
- [x] Hardware command results preserve command id, status, and message.
- [x] Hardware command results reject empty command ids.
- [x] Hardware command results reject blank messages.
- [x] Hardware connection descriptors preserve target, display name, and transport name.
- [x] Hardware connection descriptors reject blank display or transport names.
- [x] Hardware prototype catalog exposes the first planned Cartesian prototype.
- [x] The first planned hardware prototype uses an Arduino-compatible target.
- [x] The first planned hardware prototype uses stepper motors.
- [x] The first planned hardware prototype remains metadata only, not implemented.
- [x] Hardware prototype descriptors reject blank names and descriptions.

### Required Next Coverage

- [ ] Add serial connection tests when real hardware communication is implemented.

## `RobotStudio.Desktop.Tests`

### Current Coverage

- [x] Robot catalog contains the Cartesian robot.
- [x] Robot catalog keeps the expected didactic complexity order.
- [x] Robot templates expose the expected complexity levels.
- [x] Every robot template has a family.
- [x] Every robot template has at least one capability.
- [x] Implemented robot templates are openable.
- [x] Planned robot templates are not openable.
- [x] The next six teaching models are present in the expected planned order.
- [x] Planned models use `RobotViewerKind.None` and remain non-openable.
- [x] Local desktop examples cover every openable robot viewer.
- [x] Local desktop examples expose non-empty names, descriptions, and scripts.
- [x] Local desktop examples can be filtered by viewer kind.
- [x] Training viewers expose multiple local examples, including Cartesian, XY Plotter, Differential Drive, SCARA, Simple Arm, and Delta.
- [x] The 6-DOF Industrial Arm is available with a concrete 3D viewer descriptor.
- [x] The industrial arm viewer exposes multiple local `ARM6` examples.
- [x] Industrial arm frame presentation formats six joints, TCP pose, state, time, and movement explanation.
- [x] Shared non-Cartesian frame presenters format state, pose, time, frame counters, and explanations.
- [x] Desktop script validation messages summarize syntax errors with line numbers.
- [x] Desktop script validation messages explain physical limit failures.
- [x] Desktop script validation messages explain invalid command arguments.
- [x] Script editor metadata classifies `HOME`, `MOVE`, and `WAIT` lines.
- [x] Script editor metadata classifies blank and unknown lines predictably.

## Architecture Tests

- [x] Source projects follow the allowed project reference map.
- [x] Project reference names are resolved consistently from Windows and Unix directory separators.
- [x] Temporary WPF build projects are excluded from source-project architecture discovery.
- [x] `RobotStudio.Domain` has no project references, package references, or Windows target.
- [x] `RobotStudio.Desktop` is the only WPF source project.
- [x] The desktop project targets Windows and is the only WPF source project.

### Required Next Coverage

- [ ] Add UI smoke tests when the desktop workflow becomes stable enough to automate.

## `RobotStudio.Cli.Tests`

### Current Coverage

- [x] Parse `--dialect gcode` independently of its position in the command line.
- [x] Parse `--dialect=dsl` syntax.
- [x] Reject missing dialect values.
- [x] Reject unknown command-line options.
- [x] Resolve explicit `dsl`, `simple-dsl`, `gcode`, and `g-code` names.
- [x] Infer G-code from `.gcode` case-insensitively.
- [x] Default `.robot`, `.txt`, and unspecified extensions to Simple DSL.
- [x] Give an explicit dialect precedence over the file extension.
- [x] Reject unknown dialect names with the supported values in the error.

### Manual CLI Verification

Current command:

```bash
dotnet run --project src/RobotStudio.Cli
```

Expected current behavior:

- prints the RobotStudio CLI title;
- prints robot profile limits;
- prints command sequence summary;
- prints simulation timeline;
- prints final state;
- prints final position;
- prints total simulated duration.

Additional manual CLI checks:

- `dotnet run --project src/RobotStudio.Cli -- example` prints the built-in script.
- `dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian.robot` validates the example script.
- `dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian.robot` simulates the example script.
- `dotnet run --project src/RobotStudio.Cli -- validate examples/cartesian.gcode` infers and validates G-code.
- `dotnet run --project src/RobotStudio.Cli -- simulate examples/cartesian.gcode` infers and simulates G-code.
- `dotnet run --project src/RobotStudio.Cli -- example --dialect gcode` prints the built-in example as G-code.
- `dotnet run --project src/RobotStudio.Cli -- playback examples/cartesian.robot 500` prints fixed-interval playback frames.
- `dotnet run --project src/RobotStudio.Cli -- export-playback examples/cartesian.robot 500 playback.json` exports fixed-interval playback data.
- `dotnet run --project src/RobotStudio.Cli -- validate-playback playback.json` validates exported playback data.

## Desktop Verification

The first desktop viewer may be verified manually until UI behavior becomes stable enough for automated UI tests.

Current command:

```bash
dotnet run --project src/RobotStudio.Desktop
```

Expected current behavior:

- opens a WPF desktop window;
- shows a robot selection screen on startup;
- presents robot status, complexity, and capabilities as compact rectangular tags;
- keeps robot card metadata visible above the footer without clipping;
- keeps available actions and planned-release footers aligned across cards;
- adjusts robot selection cards responsively across one to six columns without forcing narrow cards;
- shows hover and keyboard focus feedback only on openable robot selection cards;
- opens an available robot from its card body, action button, or keyboard;
- represents planned models with static release status instead of disabled buttons;
- retains typed final simulation contexts for desktop `HOME` and `Reset Fault` recovery actions;
- shows recovery controls only while the active retained context is faulted;
- reuses shared script actions, playback actions, contextual recovery actions, and timeline controls across robot viewers;
- lists robot templates in the expected didactic complexity order;
- lists the Cartesian robot as available;
- lists the XY plotter as available;
- lists the differential drive robot as available;
- lists the SCARA robot as available;
- lists the simple articulated arm as available;
- lists the delta robot as available;
- lists the drone as available;
- lists the 6-DOF industrial arm as available;
- opens the Cartesian viewer from the selection screen;
- opens the Delta viewer from the selection screen;
- opens the Drone viewer from the selection screen;
- opens the 6-DOF Industrial Arm viewer from the selection screen;
- returns from the Cartesian viewer to the selection screen;
- validates the current DSL script from the Cartesian viewer;
- groups dense Cartesian viewer controls into collapsible sidebar panels;
- shows technical tooltips for dense desktop controls;
- shows didactic tooltips for workspace, TCP, homing, timeline, requested velocity, and effective velocity concepts;
- remains usable at the current minimum desktop window size;
- regenerates playback from the current DSL script;
- reports script parser or validation errors without closing the app;
- shows the current script line during playback;
- keeps the script editor height stable as commands are appended;
- shows script editor line numbers;
- shows simple command highlighting for `HOME`, `MOVE`, and `WAIT`;
- appends `HOME` when the manual home button is used;
- appends `MOVE` commands when manual jog buttons are used;
- rejects invalid manual step or speed values;
- rejects manual jog commands that exceed Cartesian axis limits;
- executes one DSL command from the command console;
- executes the command console when the user presses `Enter`;
- appends accepted console commands to the DSL script;
- records accepted and rejected console commands in the command history;
- moves one frame backward with `Prev`;
- moves one frame forward with `Next`;
- changes playback timer speed with the playback speed selector;
- jumps to command start frames from the command marker list;
- jumps to state change frames from the state marker list;
- updates the X/Y/Z position chart and current-frame cursor during playback navigation;
- updates the effective velocity chart and current-frame cursor during playback navigation;
- updates the robot state chart and current-frame cursor during playback navigation;
- updates the requested-versus-effective velocity chart during playback navigation;
- updates the accumulated distance chart during playback navigation;
- resizes the 3D viewport and side control panel with the vertical splitter;
- toggles grid, global axes, X/Y/Z labels, workspace, planned path, start/end markers, rails, carriages, and TCP/tool visibility without regenerating the simulation;
- explains the current command from the active playback frame;
- explains involved axes, requested speed, effective speed, and limiting axis for `MOVE`;
- renders the built-in Cartesian robot scene in a 3D viewport;
- renders the SCARA robot as a 3D viewport with volumetric links and joints;
- renders the Simple Articulated Arm as a 3D viewport with volumetric links and joints;
- renders the Delta Robot as a 3D viewport with triangular frame, vertical actuator rails, moving carriages, platform, TCP, and path;
- renders the Drone as a 3D viewport with flight-volume boundary, ground grid, attitude-aware rotor arms, heading indicator, and path;
- loads selected local Cartesian examples from the Cartesian viewer;
- loads selected local XY Plotter examples from the XY Plotter viewer;
- loads selected local Differential Drive examples from the mobile viewer;
- loads selected local SCARA examples from the SCARA viewer;
- loads selected local Simple Articulated Arm examples from the arm viewer;
- loads selected local Delta examples from the Delta viewer;
- loads selected local Drone examples from the Drone viewer;
- loads local `.robot` or `.txt` script files into desktop script editors;
- saves desktop script editor contents to local `.robot` or `.txt` files;
- asks the student to validate or simulate after loading a script file;
- supports keyboard shortcuts for loading, saving, validating, simulating, playback, frame stepping, zoom, and camera reset;
- avoids consuming playback/frame/camera shortcuts while focus is inside script editors or example selectors;
- zooms active 2D and 3D viewers with `Ctrl+mouse wheel`;
- explains current SCARA and Simple Articulated Arm joint-space movement from the active frame;
- explains current Delta coupled actuator-space movement from the active frame;
- explains current Drone coordinated 3D flight movement from the active frame;
- provides play and reset controls;
- provides a timeline slider;
- provides camera orbit, zoom, reset, and predefined view controls;
- rotates the camera when the user drags inside the 3D viewport;
- rotates the camera when the user starts dragging from empty viewport space, not only from rendered robot primitives;
- zooms the active viewer only when the user uses `Ctrl+mouse wheel`;
- renders workspace limits as a visible boundary instead of an opaque block over the robot;
- shows a didactic state panel with state, position, command, source line, time, and frame number;
- updates the displayed frame, time, and state while playing.
