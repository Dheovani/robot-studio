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
- [x] Use a proprietary personal-study license.
- [x] Reserve commercial, organizational, institutional, brand, redistribution, and sublicensing rights.
- [x] Do not add lesson/scenario management inside the app.
- [x] Use WPF as the first desktop viewer stack.
- [x] Do not replace the current desktop UI stack without a strong technical reason.

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
- [x] Add `CartesianRobotProfile`.
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

- [x] Review current domain type names and classify each type as general robotics or Cartesian-specific.
- [x] Decide which namespace will hold general robot concepts.
- [x] Decide which namespace will hold Cartesian robot concepts.
- [x] Introduce a general robot position concept without breaking the current Cartesian example.
- [x] Introduce a general robot profile concept without making the current code abstract too early.
- [x] Keep `CartesianPosition` as the first concrete position type.
- [x] Rename the current Cartesian profile from `RobotProfile` to `CartesianRobotProfile`.
- [x] Add tests proving the current Cartesian validation still works after the cleanup.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 3. Domain State Model

Goal: make robot execution state explicit and teachable.

- [x] Create `RobotState`.
- [x] Add state value `Idle`.
- [x] Add state value `Moving`.
- [x] Add state value `Homing`.
- [x] Add state value `Waiting`.
- [x] Add state value `Completed`.
- [x] Add state value `Faulted`.
- [x] Define which state is the initial state of a new simulation.
- [x] Define which states can transition to `Moving`.
- [x] Define which states can transition to `Waiting`.
- [x] Define which states can transition to `Homing`.
- [x] Define which states can transition to `Completed`.
- [x] Define which failures transition to `Faulted`.
- [x] Add helper for states that are actively executing work.
- [x] Add helper for states that can start normal commands.
- [x] Add helper for states that end the current command.
- [x] Add helper for recoverable states.
- [x] Add tests proving the first state values exist.
- [x] Add tests for valid state transitions.
- [x] Add tests for invalid state transitions.
- [x] Add exception for invalid state transitions.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 4. Domain Error Model

Goal: make errors clear enough for students to understand what went wrong.

- [x] Add explicit error for position outside axis limits.
- [x] Add explicit error for invalid robot command.
- [x] Add explicit error for invalid state transition.
- [x] Add explicit error for impossible movement.
- [x] Ensure each domain error message identifies the invalid value.
- [x] Ensure each domain error message identifies the expected valid range or state.
- [x] Add tests for each domain error type.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 5. Cartesian Robot Model

Goal: make the first robot model complete enough for introductory lessons.

- [x] Support X axis.
- [x] Support Y axis.
- [x] Support Z axis.
- [x] Validate minimum and maximum position per axis.
- [x] Validate maximum velocity per axis.
- [x] Add maximum acceleration per axis.
- [x] Validate maximum acceleration per axis.
- [x] Add tests for positions exactly at minimum axis limits.
- [x] Add tests for positions exactly at maximum axis limits.
- [x] Add full robot profile tests for positions exactly at minimum X/Y/Z limits.
- [x] Add full robot profile tests for positions exactly at maximum X/Y/Z limits.
- [x] Add tests for invalid axis configuration.
- [x] Add tests for invalid velocity configuration.
- [x] Add tests for invalid acceleration configuration.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 6. Motion Planner V1

Goal: keep movement planning simple, deterministic, and easy to explain.

- [x] Plan a linear movement from start position to end position.
- [x] Validate the start position.
- [x] Validate the end position.
- [x] Use the lowest maximum velocity among the involved axes.
- [x] Return zero duration for zero-distance movement.
- [x] Return no segments for zero-distance movement.
- [x] Return positive duration for non-zero movement.
- [x] Respect requested command speed when it is below axis limits.
- [x] Cap requested command speed when it is above axis limits.
- [x] Expose total movement distance in `MotionPlan`.
- [x] Expose involved axes in `MotionSegment` or equivalent type.
- [x] Add tests for single-axis movement.
- [x] Add tests for two-axis movement.
- [x] Add tests for three-axis movement.
- [x] Add tests proving the slowest involved axis limits the movement.
- [x] Add general motion planner contract.
- [x] Make current motion planner implement the general planner contract.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 7. Command Execution Model

Goal: represent command sequences before building the simulator.

- [x] Represent `MOVE` as `MoveToCommand`.
- [x] Represent `HOME` as `HomeCommand`.
- [x] Represent `WAIT` as `WaitCommand`.
- [x] Decide whether commands should carry an optional name or source line for teaching/debugging.
- [x] Add optional source metadata to robot commands.
- [x] Add a command sequence type.
- [x] Validate that a command sequence cannot contain null commands.
- [x] Validate that `WAIT` cannot have negative duration.
- [x] Validate that `MOVE` target position is checked against the robot profile before execution.
- [x] Add tests for valid command sequences.
- [x] Add tests for invalid command sequences.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 8. Simulation V1

Goal: execute commands deterministically without hardware or UI.

- [x] Add project reference from `RobotStudio.Simulation` to `RobotStudio.Domain`.
- [x] Add project reference from `RobotStudio.Simulation` to `RobotStudio.Motion`.
- [x] Create a simulation context containing robot profile, current position, current state, and current time.
- [x] Create a simulator service that receives a command sequence.
- [x] Execute `HOME` by moving to `(0, 0, 0)`.
- [x] Execute `MOVE` by using `MotionPlanner`.
- [x] Execute `WAIT` by advancing simulated time.
- [x] Update state to `Homing` while homing.
- [x] Update state to `Moving` while moving.
- [x] Update state to `Waiting` while waiting.
- [x] Update state to `Completed` after each successful command.
- [x] Update state to `Faulted` when a command fails.
- [x] Record a timeline of simulation steps.
- [x] Add tests for `HOME`.
- [x] Add tests for `MOVE`.
- [x] Add tests for `WAIT`.
- [x] Add tests for a sequence containing `HOME`, `MOVE`, and `WAIT`.
- [x] Add tests for a failing sequence.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 9. Simulation V2

Goal: prepare simulation output for timeline inspection, interpolation, and future visualization.

- [x] Add command index to each command-generated timeline step.
- [x] Add command name to each command-generated timeline step.
- [x] Keep simulation-generated timeline steps without command source.
- [x] Record command source when a command fails.
- [x] Add tests proving timeline steps preserve command source.
- [x] Add tests proving timeline records exact state transitions in order.
- [x] Add tests proving invalid initial simulation context is rejected.
- [x] Add tests proving faulted simulations preserve the last valid position.
- [x] Add tests proving zero-distance `MOVE` is simulated predictably.
- [x] Preserve command source metadata in timeline steps.
- [x] Preserve command source metadata in timeline samples.
- [x] Add timeline interpolation for Cartesian motion.
- [x] Add visual-state contract for future UI layers.
- [x] Add Cartesian simulation sample to visual-state mapper.
- [x] Add Cartesian visual-state sampler for future UI layers.
- [x] Add Cartesian playback sampler that generates visual states at fixed time intervals.
- [x] Add Cartesian workspace bounds for future 3D viewport framing.
- [x] Add Cartesian playback snapshot that packages workspace bounds, frames, duration, and failure state.
- [x] Add Cartesian robot pose mapping for the first didactic 3D mechanism model.
- [x] Add Cartesian scene frame mapping with renderable primitives for future 3D UI.
- [x] Add Cartesian viewport planning for the initial 3D camera framing.
- [x] Add versioned playback snapshot metadata for future UI compatibility.
- [x] Add playback snapshot validation for exported JSON compatibility checks.
- [x] Add tests for position sampling before the first command.
- [x] Add tests for position sampling during movement.
- [x] Add tests for position sampling during wait.
- [x] Add tests for position sampling after the final command.
- [x] Add tests for Cartesian visual-state mapping.
- [x] Add tests for Cartesian visual-state sampling.
- [x] Add tests for Cartesian playback sampling.
- [x] Add tests for Cartesian workspace bounds.
- [x] Add tests for Cartesian playback snapshot creation.
- [x] Add tests for Cartesian robot pose mapping.
- [x] Add tests for Cartesian scene frame mapping.
- [x] Add tests for Cartesian viewport planning.
- [x] Add tests for playback snapshot metadata.
- [x] Add tests for playback snapshot validation.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 10. Simple DSL V1

Goal: let students read and later write beginner-friendly robot scripts.

Initial syntax:

```txt
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
HOME
```

- [x] Add project reference from `RobotStudio.Scripting` to `RobotStudio.Domain`.
- [x] Decide whether `SPEED` is required or optional in `MOVE`.
- [x] Decide default speed if `SPEED` is optional.
- [x] Parse `HOME`.
- [x] Parse `WAIT 500`.
- [x] Parse `MOVE X=10 Y=20 Z=5`.
- [x] Parse `MOVE X=10 Y=20 Z=5 SPEED=100`.
- [x] Convert parsed `HOME` into `HomeCommand`.
- [x] Convert parsed `WAIT` into `WaitCommand`.
- [x] Convert parsed `MOVE` into `MoveToCommand`.
- [x] Report unknown command errors clearly.
- [x] Report missing coordinate errors clearly.
- [x] Report invalid number errors clearly.
- [x] Report invalid wait duration errors clearly.
- [x] Preserve script line number in parser errors.
- [x] Preserve script line number in parsed command metadata.
- [x] Preserve script text in parsed command metadata.
- [x] Report duplicate `MOVE` argument errors clearly.
- [x] Report unknown `MOVE` argument errors clearly.
- [x] Report `HOME` argument errors clearly.
- [x] Add parser tests for valid scripts.
- [x] Add parser tests for invalid scripts.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 11. CLI Learning Flow V1

Goal: provide repeatable examples students can run before writing scripts manually.

- [x] Print one hard-coded motion plan example.
- [x] Add CLI option to run the built-in Cartesian movement example.
- [x] Add CLI option to print the built-in example script.
- [x] Add CLI option to validate a script file.
- [x] Add CLI option to simulate a script file.
- [x] Add CLI option to print fixed-interval playback frames.
- [x] Add CLI option to export fixed-interval playback snapshots as JSON.
- [x] Add CLI option to validate exported playback snapshots.
- [x] Print robot profile limits in example output.
- [x] Print command sequence summary before simulation.
- [x] Print final robot state after simulation.
- [x] Print final robot position after simulation.
- [x] Print total simulated duration.
- [x] Convert domain exceptions into friendly CLI messages.
- [x] Keep CLI free from business logic.
- [x] Add CLI README examples.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.
- [x] Run `dotnet run --project src/RobotStudio.Cli`.

## 12. Hardware Boundary

Goal: prepare for real devices without implementing hardware communication too early.

- [ ] Keep `RobotStudio.Hardware` empty or placeholder-only until simulation and DSL are stable.
- [ ] Define what information a future serial command must receive.
- [ ] Define what information a future serial response must return.
- [ ] Decide whether the first hardware target will be Arduino or ESP32.
- [ ] Decide whether the first actuator model will be stepper motor or servo.
- [ ] Do not reference hardware types from `RobotStudio.Domain`.
- [ ] Do not reference hardware types from `RobotStudio.Motion`.
- [ ] Do not reference hardware types from `RobotStudio.Simulation`.

## 13. Future 3D Visualization

Goal: prepare a future visual inspection tool without influencing the current core.

- [x] Complete the pre-visual simulation readiness checklist before adding any visual framework.
- [x] Add the first WPF desktop viewer project.
- [x] Render Cartesian scene frames in a desktop 3D viewport.
- [x] Add basic playback controls for the desktop viewer.
- [x] Add camera orbit controls to the desktop viewer.
- [x] Add camera zoom and reset controls to the desktop viewer.
- [x] Add predefined camera views to the desktop viewer.
- [x] Add mouse drag orbit rotation to the desktop viewer.
- [x] Add mouse wheel zoom to the desktop viewer.
- [x] Render workspace limits as a boundary instead of an opaque block.
- [x] Add a didactic state panel to the desktop viewer.
- [x] Keep visual coordinate decisions outside the current domain model.
- [x] Define a fixed virtual environment for robot visualization.
- [x] Allow camera rotation around the robot.
- [ ] Later, show technical tooltips for complex concepts.
- [x] Ensure UI consumes simulation output instead of duplicating simulation logic.

### 13.1. Desktop Didactic Roadmap

Goal: evolve the first WPF viewer into a visual teaching tool while keeping future work centralized in this checklist.

#### 13.1.1. Cartesian Viewer Usability

- [x] Add orbit camera controls.
- [x] Add zoom control.
- [x] Add reset camera command.
- [x] Add basic predefined views: front, side, top, and isometric.
- [x] Add mouse drag orbit rotation.
- [x] Add mouse wheel zoom.
- [x] Render workspace limits without hiding the robot mechanism.
- [x] Add a state panel with time, state, position, command, and source line.
- [x] Keep the viewer consuming `CartesianPlaybackSnapshot` and `CartesianSceneFrame`.

#### 13.1.2. Robot Selection Shell

- [x] Add `RobotFamilyDescriptor`.
- [x] Add `RobotTemplate`.
- [x] Add `RobotCapability`.
- [x] Add `RobotViewerDescriptor`.
- [x] Add the first robot selection screen.
- [x] Show the Cartesian robot as `Available`.
- [x] Show articulated arm as `Planned`.
- [x] Show drone as `Planned`.
- [x] List capabilities without implementing unavailable robots.
- [x] Keep the selection screen as a simulator entry point, not an LMS.

#### 13.1.3. Initial Robot Capabilities Metadata

- [x] Add `Simulation` capability.
- [x] Add `ScriptExecution` capability.
- [x] Add `ThreeDimensionalView` capability.
- [x] Add `ManualControl` capability.
- [x] Add `HardwareCommunication` capability as metadata only.
- [x] Add `GCode` capability as metadata only.
- [x] Mark unavailable robot templates clearly in the UI.

#### 13.1.3.1. Desktop UI Polish

- [x] Replace default WPF button visuals with a dark desktop style.
- [x] Improve robot selection header hierarchy and spacing.
- [x] Render robot status as compact badges.
- [x] Render robot capabilities as compact tags.
- [x] Keep robot selection cards visually scannable at the current desktop size.
- [ ] Extract repeated desktop colors and spacing into reusable resources.
- [ ] Add hover/focus states for robot cards.
- [ ] Improve the Cartesian viewer layout with the same visual language.
- [ ] Add technical tooltips for complex controls.
- [ ] Review the desktop UI at smaller window sizes.

#### 13.1.4. Desktop Script Workflow

- [ ] Add a DSL script editor panel.
- [ ] Add line numbering to the script editor.
- [ ] Add simple command highlighting for `HOME`, `MOVE`, and `WAIT`.
- [ ] Add `Validate` button.
- [ ] Add `Simulate` button.
- [ ] Add `Play` button integration with simulated script output.
- [ ] Show parser errors with line numbers.
- [ ] Highlight the script line that produced the current playback frame.
- [ ] Keep G-code out of this milestone.

#### 13.1.5. Manual Cartesian Control

- [ ] Add `HOME` button.
- [ ] Add `X+` jog button.
- [ ] Add `X-` jog button.
- [ ] Add `Y+` jog button.
- [ ] Add `Y-` jog button.
- [ ] Add `Z+` jog button.
- [ ] Add `Z-` jog button.
- [ ] Add step size input in millimeters.
- [ ] Add requested velocity input in millimeters per second.
- [ ] Add reset simulation button.
- [ ] Add stop playback button.
- [ ] Generate simulation commands from manual actions.
- [ ] Decide whether manual actions should generate a script automatically.

#### 13.1.6. Direct Command Console

- [ ] Add a simple command input panel.
- [ ] Execute one DSL command at a time from the command input.
- [ ] Show command validation errors without crashing the desktop app.
- [ ] Append accepted commands to a visible command history.
- [ ] Reuse the same parser and simulation path used by scripts.

#### 13.1.7. Didactic Overlays

- [ ] Toggle workspace visibility.
- [ ] Toggle global axes.
- [ ] Toggle grid.
- [ ] Toggle X/Y/Z labels.
- [ ] Toggle TCP marker.
- [ ] Toggle planned path.
- [ ] Toggle start marker.
- [ ] Toggle end marker.
- [ ] Toggle robot components.

#### 13.1.8. Timeline And Movement Explanation

- [ ] Add frame-by-frame stepping.
- [ ] Add playback speed control.
- [ ] Add command markers on the timeline.
- [ ] Add state markers on the timeline.
- [ ] Add movement explanation text.
- [ ] Explain involved axes for the current movement.
- [ ] Explain requested velocity.
- [ ] Explain effective velocity.
- [ ] Explain when axis limits cap the requested velocity.
- [ ] Explain duration calculation for simple linear movement.

#### 13.1.9. Charts

- [ ] Plot X/Y/Z position over time.
- [ ] Plot effective velocity over time.
- [ ] Plot robot state over time.
- [ ] Plot requested versus effective velocity.
- [ ] Plot total distance.

#### 13.1.10. Future Interfaces Kept Out Of Scope

- [ ] Prepare G-code as a second parser dialect that produces domain commands.
- [ ] Prepare hardware command boundaries without serial implementation.
- [ ] Keep Arduino communication out until the simulator and desktop flows are stable.
- [ ] Keep ESP32 communication out until the simulator and desktop flows are stable.
- [ ] Keep real hardware execution out of the desktop viewer until hardware boundaries are designed.

#### 13.1.11. Desktop Architecture Rules

- [x] Keep `RobotStudio.Domain` free of UI, rendering, files, and hardware.
- [x] Keep `RobotStudio.Motion` free of UI.
- [x] Keep `RobotStudio.Simulation` producing contracts consumed by UI.
- [x] Keep `RobotStudio.Desktop` consuming snapshots, scene frames, poses, and viewport data.
- [x] Keep camera interaction and visual view state inside the UI layer.
- [x] Do not duplicate simulation or domain validation rules in the UI.
- [x] Keep hardware and G-code planned, not implemented, until explicitly started.

## 14. Pre-Visual Simulation Readiness

Goal: finish the deterministic simulation contract before starting visual simulation.

- [x] Define the robot execution state model.
- [x] Define the initial simulation state.
- [x] Define valid state transitions.
- [x] Define recoverable fault behavior at the state-model level.
- [x] Record a deterministic simulation timeline.
- [x] Record command index for command-generated timeline steps.
- [x] Record command name for command-generated timeline steps.
- [x] Keep simulator-generated timeline steps separate from command-generated steps.
- [x] Add Cartesian timeline sampling.
- [x] Add tests for sampling before the first command.
- [x] Add tests for sampling during movement.
- [x] Add tests for sampling during wait.
- [x] Add tests for sampling after the final command.
- [x] Add tests proving invalid initial simulation context is rejected.
- [x] Add tests proving faulted simulations preserve the last valid position.
- [x] Add tests proving zero-distance `MOVE` is simulated predictably.
- [x] Decide whether commands should carry optional source metadata for teaching and debugging.
- [x] Preserve command source metadata from DSL parsing into command execution.
- [x] Strengthen DSL validation for duplicate `MOVE` arguments.
- [x] Strengthen DSL validation for unknown `MOVE` arguments.
- [x] Strengthen DSL validation for `HOME` arguments.
- [x] Define a visual-state contract consumed by future UI layers.
- [x] Define how Cartesian simulation state maps to visual pose data.
- [x] Define Cartesian workspace bounds for future 3D viewport framing.
- [x] Keep visual pose mapping outside `RobotStudio.Domain`.
- [x] Keep visual pose mapping outside `RobotStudio.Motion`.
- [x] Run `dotnet build`.
- [x] Run `dotnet test`.

## 15. Future Robot Families

Goal: expand RobotStudio beyond the introductory Cartesian robot when the foundation is stable.

- [ ] Add an articulated robot model after Cartesian simulation is stable.
- [ ] Add a drone model after the simulator supports non-Cartesian state concepts.
- [ ] Identify which concepts can remain shared across robot families.
- [ ] Identify which concepts must be robot-family-specific.
- [ ] Add tests before each new robot family is considered complete.

## 16. Future G-Code Support

Goal: add G-code after students understand the simple DSL.

- [ ] Keep G-code out of the first DSL implementation.
- [ ] Design a G-code module for a future course section.
- [ ] Map `G28` to homing behavior.
- [ ] Map `G1` to movement behavior.
- [ ] Map `G4` to wait/dwell behavior.
- [ ] Allow DSL and G-code to produce the same domain command types.

## 17. Continuous Integration

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
