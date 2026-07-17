# TODO

## 0. Fixed Product Decisions

- [x] Define RobotStudio as a didactic robotics platform, not only a Cartesian robot simulator.
- [x] Use the generic three-axis Cartesian robot as the first supported robot model.
- [x] Keep support for articulated robots, drones, and other robot families as a future architectural goal.
- [x] Use millimeters as the standard internal distance unit for the first version.
- [x] Use a simple educational DSL before adding G-code.
- [x] Keep G-code as a future course/module topic.
- [x] Treat `HOME` as position `(0, 0, 0)` for the first Cartesian robot.
- [x] Start students with ready-made CLI examples before asking them to write scripts.
- [x] Simulate robot state, not only position.
- [x] Use technical state names in code.
- [x] Do not add lesson/scenario management inside the app.
- [x] Do not build UI yet.

## 1. Current Baseline

Goal: keep the existing first vertical slice compiling and tested before expanding the architecture.

- [x] Create `RobotStudio.Domain`.
- [x] Create `RobotStudio.Motion`.
- [x] Create `RobotStudio.Simulation`.
- [x] Create `RobotStudio.Hardware`.
- [x] Create `RobotStudio.Scripting`.
- [x] Create `RobotStudio.Cli`.
- [x] Create `RobotStudio.Domain.Tests`.
- [x] Create `RobotStudio.Motion.Tests`.
- [x] Add `Axis`.
- [x] Add `CartesianPosition`.
- [x] Add `RobotProfile`.
- [x] Add domain validation for Cartesian axis limits.
- [x] Add `MoveToCommand`.
- [x] Add `HomeCommand`.
- [x] Add `WaitCommand`.
- [x] Add `MotionPlanner`.
- [x] Add `MotionPlan`.
- [x] Add `MotionSegment`.
- [x] Add a CLI example that creates a profile and prints a motion plan.
- [x] Add initial domain tests.
- [x] Add initial motion planner tests.
- [x] Add root `README.md`.
- [x] Add root `TODO.md`.

## 2. Domain Architecture Cleanup

Goal: separate general robotics concepts from the first Cartesian robot implementation.

- [ ] Review current domain type names and classify each type as general robotics or Cartesian-specific.
- [ ] Decide which namespace will hold general robot concepts.
- [ ] Decide which namespace will hold Cartesian robot concepts.
- [ ] Introduce a general robot model concept without breaking the current Cartesian example.
- [ ] Introduce a general robot profile concept without making the current code abstract too early.
- [ ] Keep `CartesianPosition` as the first concrete position type.
- [ ] Keep `RobotProfile` working for the current Cartesian robot until a better name is chosen.
- [ ] Add tests proving the current Cartesian validation still works after the cleanup.
- [ ] Run `dotnet build`.
- [ ] Run `dotnet test`.

## 3. Domain State Model

Goal: make robot execution state explicit and teachable.

- [x] Create `RobotState`.
- [x] Add state value `Idle`.
- [x] Add state value `Moving`.
- [x] Add state value `Homing`.
- [x] Add state value `Waiting`.
- [x] Add state value `Completed`.
- [x] Add state value `Faulted`.
- [ ] Define which state is the initial state of a new simulation.
- [ ] Define which states can transition to `Moving`.
- [ ] Define which states can transition to `Waiting`.
- [x] Define which states can transition to `Homing`.
- [ ] Define which states can transition to `Completed`.
- [ ] Define which failures transition to `Faulted`.
- [x] Add tests proving the first state values exist.
- [x] Add tests for valid state transitions.
- [x] Add tests for invalid state transitions.
- [x] Add exception for invalid state transitions.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 4. Domain Error Model

Goal: make errors clear enough for students to understand what went wrong.

- [x] Add explicit error for position outside axis limits.
- [ ] Add explicit error for invalid robot command.
- [x] Add explicit error for invalid state transition.
- [ ] Add explicit error for impossible movement.
- [ ] Ensure each domain error message identifies the invalid value.
- [ ] Ensure each domain error message identifies the expected valid range or state.
- [ ] Add tests for each domain error type.
- [ ] Run `dotnet build`.
- [ ] Run `dotnet test`.

## 5. Cartesian Robot Model

Goal: make the first robot model complete enough for introductory lessons.

- [x] Support X axis.
- [x] Support Y axis.
- [x] Support Z axis.
- [x] Validate minimum and maximum position per axis.
- [x] Validate maximum velocity per axis.
- [ ] Add maximum acceleration per axis.
- [ ] Validate maximum acceleration per axis.
- [ ] Add tests for positions exactly at minimum axis limits.
- [ ] Add tests for positions exactly at maximum axis limits.
- [ ] Add tests for invalid axis configuration.
- [ ] Add tests for invalid velocity configuration.
- [ ] Add tests for invalid acceleration configuration.
- [ ] Run `dotnet build`.
- [ ] Run `dotnet test`.

## 6. Motion Planner V1

Goal: keep movement planning simple, deterministic, and easy to explain.

- [x] Plan a linear movement from start position to end position.
- [x] Validate the start position.
- [x] Validate the end position.
- [x] Use the lowest maximum velocity among the involved axes.
- [x] Return zero duration for zero-distance movement.
- [x] Return no segments for zero-distance movement.
- [x] Return positive duration for non-zero movement.
- [ ] Expose total movement distance in `MotionPlan`.
- [ ] Expose involved axes in `MotionSegment` or equivalent type.
- [ ] Add tests for single-axis movement.
- [ ] Add tests for two-axis movement.
- [ ] Add tests for three-axis movement.
- [ ] Add tests proving the slowest involved axis limits the movement.
- [ ] Run `dotnet build`.
- [ ] Run `dotnet test`.

## 7. Command Execution Model

Goal: represent command sequences before building the simulator.

- [x] Represent `MOVE` as `MoveToCommand`.
- [x] Represent `HOME` as `HomeCommand`.
- [x] Represent `WAIT` as `WaitCommand`.
- [ ] Decide whether commands should carry an optional name or source line for teaching/debugging.
- [ ] Add a command sequence type.
- [ ] Validate that a command sequence cannot contain null commands.
- [ ] Validate that `WAIT` cannot have negative duration.
- [ ] Validate that `MOVE` target position is checked against the robot profile before execution.
- [ ] Add tests for valid command sequences.
- [ ] Add tests for invalid command sequences.
- [ ] Run `dotnet build`.
- [ ] Run `dotnet test`.

## 8. Simulation V1

Goal: execute commands deterministically without hardware or UI.

- [ ] Add project reference from `RobotStudio.Simulation` to `RobotStudio.Domain`.
- [ ] Add project reference from `RobotStudio.Simulation` to `RobotStudio.Motion`.
- [ ] Create a simulation context containing robot profile, current position, current state, and current time.
- [ ] Create a simulator service that receives a command sequence.
- [ ] Execute `HOME` by moving to `(0, 0, 0)`.
- [ ] Execute `MOVE` by using `MotionPlanner`.
- [ ] Execute `WAIT` by advancing simulated time.
- [ ] Update state to `Homing` while homing.
- [ ] Update state to `Moving` while moving.
- [ ] Update state to `Waiting` while waiting.
- [ ] Update state to `Completed` after all commands finish.
- [ ] Update state to `Faulted` when a command fails.
- [ ] Record a timeline of simulation steps.
- [ ] Add tests for `HOME`.
- [ ] Add tests for `MOVE`.
- [ ] Add tests for `WAIT`.
- [ ] Add tests for a sequence containing `HOME`, `MOVE`, `WAIT`, and `MOVE`.
- [ ] Add tests for a failing sequence.
- [ ] Run `dotnet build`.
- [ ] Run `dotnet test`.

## 9. Simple DSL V1

Goal: let students read and later write beginner-friendly robot scripts.

Initial syntax:

```txt
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
HOME
```

- [ ] Add project reference from `RobotStudio.Scripting` to `RobotStudio.Domain`.
- [ ] Decide whether `SPEED` is required or optional in `MOVE`.
- [ ] Decide default speed if `SPEED` is optional.
- [ ] Parse `HOME`.
- [ ] Parse `WAIT 500`.
- [ ] Parse `MOVE X=10 Y=20 Z=5`.
- [ ] Parse `MOVE X=10 Y=20 Z=5 SPEED=100`.
- [ ] Convert parsed `HOME` into `HomeCommand`.
- [ ] Convert parsed `WAIT` into `WaitCommand`.
- [ ] Convert parsed `MOVE` into `MoveToCommand`.
- [ ] Report unknown command errors clearly.
- [ ] Report missing coordinate errors clearly.
- [ ] Report invalid number errors clearly.
- [ ] Report invalid wait duration errors clearly.
- [ ] Preserve script line number in parser errors.
- [ ] Add parser tests for valid scripts.
- [ ] Add parser tests for invalid scripts.
- [ ] Run `dotnet build`.
- [ ] Run `dotnet test`.

## 10. CLI Learning Flow V1

Goal: provide repeatable examples students can run before writing scripts manually.

- [x] Print one hard-coded motion plan example.
- [ ] Add CLI option to run the built-in Cartesian movement example.
- [ ] Add CLI option to print the built-in example script.
- [ ] Add CLI option to validate a script file.
- [ ] Add CLI option to simulate a script file.
- [ ] Print robot profile limits in example output.
- [ ] Print command sequence summary before simulation.
- [ ] Print final robot state after simulation.
- [ ] Print final robot position after simulation.
- [ ] Print total simulated duration.
- [ ] Convert domain exceptions into friendly CLI messages.
- [ ] Keep CLI free from business logic.
- [ ] Add CLI README examples.
- [ ] Run `dotnet build`.
- [ ] Run `dotnet test`.
- [ ] Run `dotnet run --project src/RobotStudio.Cli`.

## 11. Hardware Boundary

Goal: prepare for real devices without implementing hardware communication too early.

- [ ] Keep `RobotStudio.Hardware` empty or placeholder-only until simulation and DSL are stable.
- [ ] Define what information a future serial command must receive.
- [ ] Define what information a future serial response must return.
- [ ] Decide whether the first hardware target will be Arduino or ESP32.
- [ ] Decide whether the first actuator model will be stepper motor or servo.
- [ ] Do not reference hardware types from `RobotStudio.Domain`.
- [ ] Do not reference hardware types from `RobotStudio.Motion`.
- [ ] Do not reference hardware types from `RobotStudio.Simulation`.

## 12. Future 3D Visualization

Goal: prepare a future visual inspection tool without influencing the current core.

- [ ] Do not add Avalonia, WPF, MAUI, game engines, or 3D libraries yet.
- [ ] Keep visual coordinate decisions outside the current domain model.
- [ ] Later, define a fixed virtual environment for robot visualization.
- [ ] Later, allow camera rotation around the robot.
- [ ] Later, show technical tooltips for complex concepts.
- [ ] Later, ensure UI consumes simulation output instead of duplicating simulation logic.

## 13. Future Robot Families

Goal: expand RobotStudio beyond the introductory Cartesian robot when the foundation is stable.

- [ ] Add an articulated robot model after Cartesian simulation is stable.
- [ ] Add a drone model after the simulator supports non-Cartesian state concepts.
- [ ] Identify which concepts can remain shared across robot families.
- [ ] Identify which concepts must be robot-family-specific.
- [ ] Add tests before each new robot family is considered complete.

## 14. Future G-Code Support

Goal: add G-code after students understand the simple DSL.

- [ ] Keep G-code out of the first DSL implementation.
- [ ] Design a G-code module for a future course section.
- [ ] Map `G28` to homing behavior.
- [ ] Map `G1` to movement behavior.
- [ ] Map `G4` to wait/dwell behavior.
- [ ] Allow DSL and G-code to produce the same domain command types.

## 15. Continuous Integration

Goal: make every pushed change validate the project automatically.

- [x] Create GitHub Actions workflow directory.
- [x] Add CI workflow for pushes and pull requests.
- [x] Add manual workflow dispatch support.
- [x] Restore the solution in CI.
- [x] Build the solution in Release mode in CI.
- [x] Run xUnit tests in Release mode in CI.
- [x] Upload test result artifacts.
- [x] Run `dotnet format` in verification mode.
- [x] Document CI behavior in `docs/ci.md`.
- [ ] Add code coverage reporting when coverage goals are defined.
- [ ] Add stricter analyzers when the coding standard becomes more mature.
- [ ] Add package vulnerability scanning when external dependencies become more relevant.
