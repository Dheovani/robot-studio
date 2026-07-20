# Technical Decisions

## Accepted Decisions

### RobotStudio Is A Didactic Robotics Platform

RobotStudio must not be treated as only a Cartesian robot simulator. The first supported robot is a generic three-axis Cartesian robot, but the architecture should allow future robot families such as articulated robots and drones.

The domain exposes small general contracts for robot positions and profiles. Concrete robot families should implement these contracts instead of forcing every robot into Cartesian assumptions.

### First Robot Model

The first robot model is a generic introductory Cartesian robot with X, Y, and Z axes. It is not modeled as a CNC machine, 3D printer, plotter, or pick-and-place machine yet.

### Units

The initial system uses millimeters as the standard internal distance unit. The initial motion vocabulary uses millimeters per second for velocity. Unit conversion is intentionally out of scope for the first version.

Cartesian axis acceleration is represented in millimeters per second squared.

### Home Position

For the first Cartesian robot, `HOME` means position `(0, 0, 0)`.

### Error Style

The domain may throw explicit domain exceptions when an operation is invalid. Error messages should help students understand the invalid value and the expected valid range or state.

CLI and future scripting layers should catch domain errors and present friendly messages.

Domain exceptions live under the `RobotStudio.Domain.Exceptions` namespace.

Command validation errors should use `InvalidRobotCommandException` when the command itself is malformed, such as a negative `WAIT` duration or a non-positive requested movement speed.

Motion planning may use `ImpossibleMovementException` when positions and profile data are valid, but the planner still cannot produce a meaningful executable movement.

### Motion Planning

The first motion planner is intentionally simple. It plans a linear movement, validates positions, estimates duration, and uses the lowest maximum velocity among involved axes.

When a movement command provides a requested speed, the planner uses the lower value between the requested speed and the involved axis limits. This keeps scripts expressive without allowing them to bypass physical constraints.

Motion plans expose total movement distance, and motion segments expose the involved axes. These values are useful for CLI output, tests, future visualization, and classroom explanations.

Axis acceleration limits are part of the robot profile but are not used by the first linear planner yet. They are present so the physical profile is complete before acceleration-aware planning is introduced.

Motion planning uses a generic planner contract so future robot families can provide their own movement logic. The current planner implements that contract for the Cartesian position/profile pair.

Advanced robotics physics is out of scope for now.

Out of scope:

- S-curve planning.
- Jerk-limited motion.
- PID control.
- Inverse kinematics.
- Collision detection.

### Simulation

The simulator must eventually model both position and robot state. State names are technical and code-oriented:

- `Idle`
- `Moving`
- `Homing`
- `Waiting`
- `Completed`
- `Faulted`

`HOME` may move the robot into `Homing` from any state. `Completed` is not a terminal state; the robot may keep receiving commands after a completed command sequence. `Faulted` is recoverable, initially through either `Idle` or `Homing`, while the exact recovery command remains open. Invalid state transitions must be explicit and use `InvalidRobotStateTransitionException`.

The initial simulation state is `Idle`. Normal commands may start from `Idle` or `Completed`. The active execution states are `Moving`, `Homing`, and `Waiting`. A command ends in either `Completed` or `Faulted`.

The allowed first-version transitions are:

- `Idle` to `Moving`, `Homing`, `Waiting`, or `Faulted`.
- `Moving` to `Homing`, `Completed`, or `Faulted`.
- `Homing` to `Homing`, `Completed`, or `Faulted`.
- `Waiting` to `Homing`, `Completed`, or `Faulted`.
- `Completed` to `Idle`, `Moving`, `Homing`, `Waiting`, or `Faulted`.
- `Faulted` to `Idle` or `Homing`.

### Scripting

The first scripting format is a simple educational DSL, not G-code.

Initial target syntax:

```txt
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
HOME
```

G-code support is planned for a future course module and should eventually produce the same domain command types as the simple DSL.

### UI And Visualization

No UI should be added yet. Future 3D visualization should observe simulation output instead of defining business rules.

Visual coordinate and camera decisions should remain outside the current domain model until the visual layer exists.

### Hardware

Hardware communication is not part of the first implementation. Future hardware work should live in `RobotStudio.Hardware` and must not leak into `RobotStudio.Domain`.

Likely future targets include Arduino or ESP32 with educational actuators such as stepper motors or servos.

## Open Decisions

- Exact namespace split between general robotics concepts and Cartesian-specific concepts.
- Whether DSL `MOVE` requires `SPEED` or uses a default speed.
- Whether commands should carry optional source information, such as script line numbers, for teaching and debugging.
- Exact user-facing recovery command for `Faulted`.
- First real hardware target: Arduino or ESP32.
- First actuator model: stepper motor or servo.
