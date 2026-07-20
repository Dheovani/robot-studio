# Test Map

This document maps expected automated tests to project behavior. It should be updated whenever new behavior is added.

## `RobotStudio.Domain.Tests`

### Current Coverage

- [x] Valid Cartesian position inside limits does not throw.
- [x] Cartesian position outside limits throws `PositionOutOfRangeException`.
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

- [ ] Full robot profile validates positions exactly at minimum X/Y/Z limits.
- [ ] Full robot profile validates positions exactly at maximum X/Y/Z limits.

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

### Required Next Coverage

- [x] Timeline records exact state transitions in order.

## `RobotStudio.Scripting.Tests`

### Current Coverage

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
