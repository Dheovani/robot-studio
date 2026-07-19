# Test Map

This document maps expected automated tests to project behavior. It should be updated whenever new behavior is added.

## `RobotStudio.Domain.Tests`

### Current Coverage

- [x] Valid Cartesian position inside limits does not throw.
- [x] Cartesian position outside limits throws `PositionOutOfRangeException`.
- [x] Robot state exposes the first execution state values.
- [x] `HOME` can transition from every robot state to `Homing`.
- [x] Active states can transition to `Completed`.
- [x] Invalid state transitions return `false`.
- [x] Invalid enforced state transitions throw `InvalidRobotStateTransitionException`.
- [x] Command validator accepts `HOME`.
- [x] Command validator accepts `WAIT`.
- [x] Command validator validates `MOVE` target position.
- [x] Null command sequence input is rejected.
- [x] Empty command sequence is rejected.
- [x] Command sequence containing null command is rejected.
- [x] Valid command sequence preserves command order.

### Required Next Coverage

- [ ] Position exactly at minimum X/Y/Z limits is valid.
- [ ] Position exactly at maximum X/Y/Z limits is valid.
- [ ] Invalid axis configuration is rejected.
- [ ] Invalid maximum velocity is rejected.
- [ ] Invalid maximum acceleration is rejected after acceleration is added.
- [ ] `WaitCommand` rejects negative duration.

## `RobotStudio.Motion.Tests`

### Current Coverage

- [x] Planner creates a plan for valid movement.
- [x] Planner rejects target position outside limits.
- [x] Non-zero displacement has positive duration.
- [x] Start position equal to end position returns a stationary plan.

### Required Next Coverage

- [ ] Single-axis movement uses that axis velocity limit.
- [ ] Two-axis movement uses the slowest involved axis.
- [ ] Three-axis movement uses the slowest involved axis.
- [ ] Motion plan exposes total distance after distance is added.
- [ ] Motion segment exposes involved axes after involved axes are added.

## `RobotStudio.Simulation.Tests`

### Current Coverage

- [x] New simulation context starts in `Idle`.
- [x] `HOME` moves the robot to origin and ends in `Completed`.
- [x] `MOVE` updates final position and ends in `Completed`.
- [x] `WAIT` advances simulated time without moving.
- [x] A sequence containing `HOME`, `MOVE`, and `WAIT` executes in order.
- [x] Failing command sequence ends in `Faulted`.

### Required Next Coverage

- [ ] Timeline records exact state transitions in order.
- [ ] Invalid initial simulation context is rejected.
- [ ] Faulted simulations preserve the last valid position.
- [ ] Zero-distance `MOVE` is simulated predictably.

## Future `RobotStudio.Scripting.Tests`

- [ ] Parse `HOME`.
- [ ] Parse `WAIT 500`.
- [ ] Parse `MOVE X=10 Y=20 Z=5`.
- [ ] Parse `MOVE X=10 Y=20 Z=5 SPEED=100`.
- [ ] Unknown command reports a clear parser error.
- [ ] Missing coordinate reports a clear parser error.
- [ ] Invalid number reports a clear parser error.
- [ ] Invalid wait duration reports a clear parser error.
- [ ] Parser errors preserve script line number.

## CLI Verification

The CLI may be verified manually until CLI behavior becomes complex enough to require automated tests.

Current command:

```bash
dotnet run --project src/RobotStudio.Cli
```

Expected current behavior:

- prints the RobotStudio motion plan title;
- prints start and end Cartesian positions;
- prints segment count;
- prints total duration;
- prints linear segment velocity and duration.
