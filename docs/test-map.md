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
- [x] Planner rejects impossible movement when distance exists but no axis displacement is measurable.

### Required Next Coverage

- [x] Single-axis movement uses that axis velocity limit.
- [x] Two-axis movement uses the slowest involved axis.
- [x] Three-axis movement uses the slowest involved axis.

## `RobotStudio.Simulation.Tests`

### Current Coverage

- [x] New simulation context starts in `Idle`.
- [x] `HOME` moves the robot to origin and ends in `Completed`.
- [x] `MOVE` updates final position and ends in `Completed`.
- [x] `MOVE` with requested velocity uses that velocity for duration.
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
- [x] Timeline sampling during wait keeps the same position.
- [x] Timeline sampling after the final step returns the final position.
- [x] Cartesian visual-state mapping preserves position in millimeters.
- [x] Cartesian visual-state mapping preserves state and command metadata.
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

### Required Next Coverage

- [x] Timeline records exact state transitions in order.

## `RobotStudio.Scripting.Tests`

### Current Coverage

- [x] The Simple DSL parser implements the script dialect contract.
- [x] The Simple DSL dialect is marked as available.
- [x] The G-code dialect is marked as planned.
- [x] Parse `HOME`.
- [x] Parse `WAIT 500`.
- [x] Parse `MOVE X=10 Y=20 Z=5`.
- [x] Parse `MOVE X=10 Y=20 Z=5 SPEED=100`.
- [x] Unknown command reports a clear parser error.
- [x] Missing coordinate reports a clear parser error.
- [x] Invalid number reports a clear parser error.
- [x] Invalid wait duration reports a clear parser error.
- [x] Parser errors preserve script line number.
- [x] Parsed commands preserve script line number metadata.
- [x] Parsed commands preserve script text metadata.
- [x] Duplicate MOVE argument reports a clear parser error.
- [x] Unknown MOVE argument reports a clear parser error.
- [x] HOME with arguments reports a clear parser error.

### Required Next Coverage

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

### Required Next Coverage

## `RobotStudio.Desktop.Tests`

### Current Coverage

- [x] Robot catalog contains the Cartesian robot.
- [x] Robot catalog keeps the expected didactic complexity order.
- [x] Robot templates expose the expected complexity levels.
- [x] Every robot template has a family.
- [x] Every robot template has at least one capability.
- [x] The Cartesian robot is the only openable robot template.
- [x] Planned robot templates are not openable.
- [x] Script editor metadata classifies `HOME`, `MOVE`, and `WAIT` lines.
- [x] Script editor metadata classifies blank and unknown lines predictably.

### Required Next Coverage

## CLI Verification

The CLI may be verified manually until CLI behavior becomes complex enough to require automated tests.

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
- presents robot status and complexity as badges and capabilities as tags;
- keeps robot card badges visible above the footer button without clipping;
- keeps robot card footer buttons aligned across cards;
- adjusts robot selection cards responsively across one, two, and three columns;
- shows hover and keyboard focus feedback on robot selection cards;
- lists robot templates in the expected didactic complexity order;
- lists the Cartesian robot as available;
- lists the XY plotter, differential drive robot, SCARA robot, simple articulated arm, delta robot, drone, and 6-DOF industrial arm as planned;
- opens the Cartesian viewer from the selection screen;
- returns from the Cartesian viewer to the selection screen;
- validates the current DSL script from the Cartesian viewer;
- groups dense Cartesian viewer controls into collapsible sidebar panels;
- shows technical tooltips for dense desktop controls;
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
- provides play and reset controls;
- provides a timeline slider;
- provides camera orbit, zoom, reset, and predefined view controls;
- rotates the camera when the user drags inside the 3D viewport;
- zooms the camera when the user uses the mouse wheel inside the 3D viewport;
- renders workspace limits as a visible boundary instead of an opaque block over the robot;
- shows a didactic state panel with state, position, command, source line, time, and frame number;
- updates the displayed frame, time, and state while playing.
