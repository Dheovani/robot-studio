# Technical Decisions

## Accepted Decisions

### RobotStudio Is A Didactic Robotics Platform

RobotStudio must not be treated as only a Cartesian robot simulator. The first supported robot is a generic three-axis Cartesian robot, but the architecture should allow future robot families such as articulated robots and drones.

The domain exposes small general contracts for robot positions and profiles. Concrete robot families should implement these contracts instead of forcing every robot into Cartesian assumptions.

General robotics concepts stay in `RobotStudio.Domain`. This includes robot state, state transitions, command abstractions, command sequences, source metadata, general position/profile contracts, and domain exceptions.

Cartesian-specific concepts stay in `RobotStudio.Domain.Cartesian`. This includes `Axis`, `AxisId`, `CartesianPosition`, and `CartesianRobotProfile`.

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

The Cartesian and XY plotter planners validate positions, plan linear movement, and use the lowest maximum velocity and acceleration limits among involved axes.

When a movement command provides a requested speed, the planner uses the lower value between the requested speed and the involved axis limits. This keeps scripts expressive without allowing them to bypass physical constraints.

Motion plans expose total movement distance, and motion segments expose the involved axes. These values are useful for CLI output, tests, visualization, and classroom explanations.

`TrapezoidalMotionProfile` is a reusable scalar profile that operates on any consistent distance, velocity, and acceleration units. It calculates acceleration, optional constant-velocity, and deceleration phases. Short movements become triangular profiles because they must decelerate before reaching the configured velocity limit. Cartesian and XY plotter planners use millimeters, millimeters per second, and millimeters per second squared. SCARA, Simple Articulated Arm, and 6-DOF Industrial Arm planners use degrees, degrees per second, and degrees per second squared.

The profile remains independent of robot topology. Each family-specific planner is responsible for selecting meaningful limits and units before creating a profile. Articulated planners synchronize the movement using the maximum joint travel and the lowest velocity and acceleration limits among the joints that move. Every joint then follows the same normalized profile progress, so coordinated joints start and finish together without pretending that angular movement is linear distance.

Cartesian simulation steps may carry the planned scalar profile as timeline metadata. `SimulationTimelineSampler` uses its normalized distance progress to interpolate position, so rendered playback visibly accelerates and decelerates. Cartesian visual states expose exact profile phase, scalar velocity, and scalar acceleration in millimeter-based units.

Articulated simulation steps may also carry their planned scalar profile as timeline metadata. Their playback samplers use normalized angular progress to interpolate every joint consistently. This keeps acceleration behavior in motion and simulation layers while the desktop viewer remains a consumer of deterministic frames.

Differential-drive planning keeps translation and rotation as sequential segments with independent scalar profiles. Translation uses millimeters, millimeters per second, and millimeters per second squared; rotation uses degrees, degrees per second, and degrees per second squared. The simulator records the pose at the boundary between segments, and playback completes translation before changing heading. This preserves the planner's intended mobile motion instead of blending incompatible linear and angular units into one profile.

Differential-drive odometry is derived in `RobotStudio.Simulation` from each simulated pose change, wheelbase, and wheel radius. It accumulates signed left/right wheel travel and wheel rotation, and playback calculates matching intermediate odometry for acceleration-aware frames. This first model is ideal and deterministic: it does not include encoder quantization, wheel slip, measurement noise, or pose-estimation uncertainty. Those effects require explicit future sensor and dynamics models rather than hidden randomness.

Delta planning synchronizes all involved linear actuators using the maximum actuator travel and the lowest velocity and acceleration limits among those actuators. Every actuator follows the same normalized millimeter-based profile progress so the parallel mechanism remains coordinated.

Drone planning keeps independent profiles for 3D translation, roll/pitch attitude, and yaw because they use different units and limits. Roll and pitch share one profile based on their maximum angular change, while yaw retains its continuous-heading profile. All profiles occur during one coordinated segment whose duration is the longest profile duration. Playback time-scales shorter profile progress to the shared segment duration, causing position and attitude to finish together while staying below their configured velocity and acceleration limits. This remains a simplified kinematic teaching model; thrust, dynamics, stabilization, PID control, wind, and aerodynamic physics are future work.

Cartesian playback snapshot format version 2 adds these motion metrics to each visual frame. The validator accepts both versions 1 and 2 so existing exported lessons remain usable; missing version 1 metrics deserialize to zero and no profile phase. New version 2 snapshots validate finite acceleration and finite, non-negative velocity. Future additions must follow the same explicit versioning and compatibility-test process.

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

`HOME` may move the robot into `Homing` from any state. `Completed` is not a terminal state; the robot may keep receiving commands after a completed command sequence. `Faulted` is recoverable through `RESET` or `HOME`. Invalid state transitions must be explicit and use `InvalidRobotStateTransitionException`.

`RESET` is represented by `ResetFaultCommand` and is valid only while the simulation is `Faulted`. A caller resumes execution from the failed result's `FinalContext`; resetting changes only the logical state to `Idle`, preserving Cartesian pose, mobile pose and odometry, articulated joints, parallel actuators, aerial attitude, and elapsed simulated time. `HOME` is intentionally different because it performs a planned physical movement to the robot family's origin. The failed result and its timeline remain available as the immutable history of the previous execution.

The desktop application retains the typed `FinalContext` produced by the latest executed script for each active robot family. Its session `HOME` and `Reset Fault` actions execute a new one-command sequence from that context and build a new playback from the recovery result. Validation remains side-effect free and does not replace the retained session context.

The initial simulation state is `Idle`. Normal commands may start from `Idle` or `Completed`. The active execution states are `Moving`, `Homing`, and `Waiting`. A command ends in either `Completed` or `Faulted`.

The allowed first-version transitions are:

- `Idle` to `Moving`, `Homing`, `Waiting`, or `Faulted`.
- `Moving` to `Homing`, `Completed`, or `Faulted`.
- `Homing` to `Homing`, `Completed`, or `Faulted`.
- `Waiting` to `Homing`, `Completed`, or `Faulted`.
- `Completed` to `Idle`, `Moving`, `Homing`, `Waiting`, or `Faulted`.
- `Faulted` to `Idle` or `Homing`.

Simulation timeline steps may include the zero-based command index and command name that produced the step. Steps generated by the simulator itself, such as the initial "simulation started" step, do not have command source metadata.

The CLI may display command indices as one-based values because that is friendlier for students reading output.

Simulation sampling is handled by `SimulationTimelineSampler`. Sampling clamps times before the first step to the first known state and times after the final step to the final known state. During active command intervals, Cartesian positions are linearly interpolated between timeline steps.

Zero-distance `MOVE` commands still produce normal command timeline steps and complete without advancing simulated time. If a command fails, the simulator keeps the last valid position and records the failure at that position.

Cartesian workspace obstacles are simulation-environment concepts, not robot-profile limits and not rendering primitives. `CartesianSimulationEnvironment` stores immutable axis-aligned obstacle volumes, while `CartesianPathCollisionDetector` performs deterministic segment/AABB intersection and reports the first collision point and trajectory fraction. `RobotSimulator` checks both `MOVE` and `HOME` paths before changing physical state. An obstruction produces `CartesianPathObstructedException`, transitions the simulation to `Faulted`, and preserves the last valid position and elapsed time.

This first collision model intentionally represents the Cartesian TCP as a point moving along a linear segment. It does not yet model tool volume, rail/carriage self-collision, moving robot links, or swept volumes. Other robot families must receive topology-appropriate collision models rather than reusing Cartesian point-path assumptions.

The Differential Drive Robot uses a separate planar collision model. Its profile declares an explicit `CollisionRadiusMillimeters` instead of deriving body size from wheel geometry. `PlanarSimulationEnvironment` contains rectangular obstacles, and `CircularFootprintCollisionDetector` computes the earliest contact between the robot's swept circular footprint and obstacle sides or rounded corners. Because the footprint is rotationally symmetric, in-place heading changes do not require a separate orientation-dependent body model. An obstructed `DRIVE` or `HOME` faults before movement begins and therefore preserves pose, odometry, and elapsed time.

SCARA collision checks reuse the planar environment but not the mobile footprint rule. The SCARA profile declares a physical link collision radius, and each link is modeled as a capsule from base to elbow or elbow to TCP. `ScaraLinkCollisionDetector` tests complete capsule geometry against every rectangular obstacle at deterministic joint-space samples no more than one degree apart by default. It identifies `FirstLink` or `SecondLink` semantically and reports the sampled joint position and trajectory fraction. The sampler shares `ScaraJointInterpolation` with playback so both systems follow the same coordinated joint path.

SCARA movement collision is intentionally sampled rather than a mathematically continuous swept-volume solution. Smaller configurable angular steps increase detection resolution at a computational cost. The default is appropriate for the introductory deterministic model, while continuous collision detection remains future advanced work.

The remaining 3D families use `SpatialSimulationEnvironment` and immutable axis-aligned `SpatialObstacle` volumes. Shared spatial code performs only envelope geometry; each family remains responsible for deriving meaningful physical components from its own state and kinematics.

- The Simple Articulated Arm samples its three planar link envelopes through joint space.
- The 6-DOF Industrial Arm derives the base column, upper arm, forearm, and wrist/tool chain in 3D and samples those link envelopes through six-joint movement.
- The Delta Robot samples all actuator coordinates, derives three carriage-to-platform links plus the moving platform, and tests each parallel component.
- The Drone uses an explicit spherical body radius and tests its complete 3D center trajectory; roll, pitch, and yaw do not change that rotationally symmetric introductory envelope.

Spatial link and component checks conservatively expand axis-aligned obstacles by the configured component radius. Articulated movement uses a default maximum one-degree joint step, while Delta movement uses a default maximum two-millimeter actuator step. These deterministic safety envelopes favor understandable and repeatable behavior over production-grade continuous collision detection. More exact mesh, self-collision, and swept-volume analysis belongs to the future advanced rendering/simulation work.

Robot commands may carry optional source metadata through `RobotCommandSource`. The simple DSL uses this metadata to preserve the source line number and original command text. Simulation timeline steps and timeline samples propagate this metadata so CLI output and future visual tools can explain where a command came from.

Future visual layers should consume `RobotVisualState` instead of reading low-level simulation internals directly. The first mapper, `CartesianVisualStateMapper`, converts Cartesian simulation samples into a visual position expressed in millimeters. Visual pose mapping stays in `RobotStudio.Simulation`; it must not be added to `RobotStudio.Domain` or `RobotStudio.Motion`.

`CartesianVisualStateSampler` is the preferred entry point for future Cartesian visual playback. It combines timeline sampling and visual-state mapping so UI layers can request a `RobotVisualState` for a given simulation time without coordinating lower-level simulation services directly.

`CartesianPlaybackSampler` generates fixed-interval visual states from a completed simulation result. It always includes the final simulation frame, even when the interval does not land exactly on the final timestamp. This gives the CLI and future UI layers a stable playback contract.

`CartesianWorkspaceBounds` derives the visual workspace limits from the Cartesian robot profile. Future 3D viewports should use these bounds to frame the robot workspace instead of hard-coding scene dimensions in the UI.

`CartesianPlaybackSnapshot` packages workspace bounds, fixed-interval visual frames, total duration, success status, and an optional failure message. The CLI can export this snapshot as JSON so future visual tools can consume the same simulation result without duplicating simulation rules.

`CartesianRobotPoseMapper` converts a visual TCP position into a simple didactic mechanism pose. The first convention keeps the base at `(0, 0, 0)`, places the X carriage at `(x, 0, 0)`, the Y carriage at `(x, y, 0)`, and the Z carriage/tool center point at `(x, y, z)`. This is intentionally a teaching model for visualization, not a detailed CAD assembly.

`CartesianSceneFrameMapper` converts a Cartesian robot pose into simple renderable primitives. The first scene format uses named boxes with an identifier, primitive kind, center, and size. Future UI layers should render these primitives instead of deriving robot geometry from raw coordinates.

`CartesianViewportPlanner` derives an initial camera target, camera position, up direction, and clipping distances from the workspace bounds. This keeps first-load scene framing deterministic while leaving camera interaction and rendering technology for the future UI layer.

Playback snapshots include `PlaybackSnapshotMetadata` with a format version, robot family, distance unit, time unit, and sample interval. Future UI and tooling should check this metadata before consuming snapshot contents so the export format can evolve deliberately.

`PlaybackSnapshotValidator` validates exported playback snapshots before a UI or external tool consumes them. It checks supported metadata versions, required sections, frame/pose/scene-frame count consistency, non-negative duration, and version 2 motion metrics.

### Scripting

The first scripting format is a simple educational DSL, not G-code.

Script parsing is exposed through `IRobotScriptDialect`. A dialect receives script text and produces a `RobotCommandSequence`. The current `RobotScriptParser` implements this contract as the available Simple DSL dialect for `HOME`, fault recovery `RESET`, Cartesian `MOVE`, mobile `DRIVE`, SCARA joint commands, simple arm joint commands, six-joint industrial arm commands, Delta actuator commands, Drone pose commands, and `WAIT`. G-code is represented as a planned dialect descriptor, but no G-code parser is implemented yet.

Initial target syntax:

```txt
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
HOME
```

G-code support is planned for a future course module and should eventually produce the same domain command types as the simple DSL.

### UI And Visualization

The first desktop UI uses WPF and targets Windows. This keeps the first visual iteration package-free and focused on rendering the simulation contract already produced by `RobotStudio.Simulation`.

The core libraries and CLI are validated separately through `build/RobotStudio.Portable.slnx`, which excludes WPF desktop projects and runs in CI on Windows, Linux, and macOS. Cross-platform desktop UI support remains a future product decision.

Architecture tests live in `RobotStudio.Architecture.Tests`. They read project files as XML and guard the allowed project reference map, the purity of `RobotStudio.Domain`, the WPF-only desktop boundary, and the portable solution's exclusion of WPF projects. These tests are intentionally package-free so the architecture rules remain easy to inspect in class.

The desktop viewer must consume simulation output, especially scene frames and viewport data, instead of duplicating simulation, motion, or geometry rules.

Cross-family simulation contracts belong in `RobotStudio.Simulation`. `IRobotPlaybackFrame`, `IRobotPlaybackSnapshot`, and `IRobotPlaybackSnapshot<TFrame>` expose shared timeline metadata such as time, state, command source, duration, frame count, and success/failure without forcing Cartesian positions, mobile poses, or articulated joints into one shape. Family-specific snapshots keep their strongly typed frame lists, while shared tools can use `RobotPlaybackSummary` when only high-level playback information is needed.

Visual coordinate and camera decisions must remain outside the domain model.

Reusable viewport infrastructure belongs in `RobotStudio.Desktop.Rendering`. Orbit camera construction, angular normalization, pointer-drag interaction state, scene lighting, and basic mesh generation are desktop rendering concerns, not domain or simulation rules. Robot-specific viewers should decide which conceptual parts to show, while shared rendering helpers should handle repeated WPF `Viewport3D` mechanics such as cameras, lights, mouse capture, drag deltas, boxes, oriented links, planar grids, reachable workspace rings, and simple volumetric markers.

Milestone 9 will add a realistic visualization path without replacing the current schematic renderer. Schematic, realistic, and realistic-with-educational-overlays modes must consume simulation state through renderer-neutral contracts; no renderer may become the source of truth for robot state. Graphics-library objects, asset formats, cameras, materials, lighting, hit testing, and rendering frame timing remain desktop concerns.

Future rendering dependencies must be open source and usable without paid licenses, subscriptions, royalties, or commercial licensing fees. HelixToolkit is the preferred candidate, Stride is the advanced alternative, and Veldrid is a low-level fallback rather than the preferred path. Ab4d.SharpEngine is rejected as the default because its licensing model can introduce commercial licensing requirements. The evaluation must be repeated when Milestone 9 starts, and no rendering dependency is added during planning. glTF 2.0/GLB is the preferred future model asset direction. The complete milestone specification is in [Advanced 3D Visualization](advanced-3d-visualization.md).

Local example metadata belongs in `RobotStudio.Desktop.Examples`. Examples are product/UI teaching assets that provide starter scripts for available viewers. They should not be hard-coded inside individual event handlers, and they should remain separate from parser or simulator rules.

Shared viewer presentation logic belongs in `RobotStudio.Desktop.Viewers`. The WPF window may still own concrete control events and drawing calls, but repeated formatting of playback state, frame counters, command names, footer text, and didactic explanations should be moved into small presenter types that can be tested without launching WPF.

Desktop script validation messages are formatted in `RobotStudio.Desktop.Scripting`. The parser and domain still throw explicit technical exceptions, while the desktop layer translates them into concise student-facing summaries with a probable category, the original detail, and a suggested next action.

The desktop start screen uses a didactic robot catalog made of family descriptors, template descriptors, availability status, capabilities, viewer descriptors, and complexity levels. This catalog is product metadata for navigation and learning progression; it does not implement robot simulation behavior.

Desktop viewers share reusable templates for script actions, playback actions, and contextual fault recovery. Non-Cartesian viewers also share a `ViewerTimeline` control. Recovery actions remain hidden during normal operation and are presented in the script panel only while the retained session is faulted.

The current didactic order is:

- Cartesian Robot.
- XY Plotter.
- Differential Drive Robot.
- Cylindrical Robot.
- Ackermann Steering Robot.
- SCARA Robot.
- Simple Articulated Arm.
- Omnidirectional Robot.
- Delta Robot.
- Drone.
- Self-Balancing Robot.
- 6-DOF Industrial Arm.
- Stewart Platform.
- Mobile Manipulator.

Only templates marked as `Available` and backed by a concrete viewer are openable. The Cartesian robot, XY plotter, differential drive robot, SCARA robot, Simple Articulated Arm, Delta Robot, Drone, and 6-DOF Industrial Arm are available now. Future templates must remain planned metadata until they have a concrete viewer.

The planned roadmap adds two catalog families only where they represent a distinct architectural model: `Cylindrical` for mechanisms that mix rotary and linear joints in a cylindrical workspace, and `Hybrid` for robots composed from independently meaningful subsystems. Ackermann Steering, Omnidirectional, and Self-Balancing robots remain in the `Mobile` family; the Stewart Platform remains `Parallel`. Planned capability metadata describes the intended teaching scope but does not imply implemented simulation behavior.

The XY plotter is modeled as a two-axis robot with its own `XYPlotterPosition`, `XYPlotterProfile`, and `XYPlotterMotionPlanner`. The desktop viewer maps it onto a fixed `Z=0` drawing plane so the current visual playback pipeline can be reused without pretending the domain model has a real Z axis.

The differential drive robot is modeled as a mobile robot with `DifferentialDrivePose`, `DifferentialDriveProfile`, `DifferentialDriveMoveCommand`, `DifferentialDriveMotionPlanner`, `DifferentialDriveSimulator`, and `DifferentialDrivePlaybackSampler`. Its motion plan separates translation from rotation so mobile movement is not forced into the Cartesian linear planner. The first desktop viewer is 2D because heading and planar navigation are the core teaching concepts for this robot family.

The SCARA robot is modeled as an articulated planar robot with `ScaraJointPosition`, `ScaraRobotProfile`, `ScaraKinematics`, `ScaraMoveJointsCommand`, `ScaraMotionPlanner`, `ScaraSimulator`, and `ScaraPlaybackSampler`. It introduces joint-space movement and forward/inverse kinematics without forcing articulated robots through Cartesian or mobile motion contracts. The first desktop viewer is 3D and renders a volumetric base, two horizontal links, joints, reachable workspace, tool path, and tool marker.

The Simple Articulated Arm is modeled as a three-joint planar arm with `SimpleArmJointPosition`, `SimpleArmRobotProfile`, `SimpleArmKinematics`, `SimpleArmMoveJointsCommand`, `SimpleArmMotionPlanner`, `SimpleArmSimulator`, and `SimpleArmPlaybackSampler`. It intentionally starts with forward kinematics only so students can first learn how base, shoulder, and elbow angles compose into a tool pose before inverse kinematics is introduced. The first desktop viewer is 3D and renders a volumetric base, three links, joints, reachable workspace, tool path, and tool orientation.

The 6-DOF Industrial Arm uses explicit `J1` through `J6` joint coordinates, individual limits, and coordinated joint-space planning. Its first forward-kinematics model represents base yaw, shoulder/elbow/wrist pitch, and wrist/tool roll as a simplified serial chain. This is intended to teach six-joint composition and TCP orientation before Denavit-Hartenberg parameters, inverse kinematics, singularities, dynamics, or collision checks are introduced. Its 3D viewer renders a serial link chain, six joint markers, TCP orientation, reachable floor area, path playback, state, and local `ARM6` examples while consuming the industrial-arm playback snapshot.

The Delta Robot starts as a simplified parallel robot model with `DeltaActuatorPosition`, `DeltaRobotProfile`, `DeltaKinematics`, `DeltaMoveActuatorsCommand`, `DeltaMotionPlanner`, `DeltaSimulator`, and `DeltaPlaybackSampler`. The initial educational model uses three vertical actuators named A/B/C and maps actuator differences to X/Y tool displacement while the actuator average drives Z. This deliberately teaches parallel coupling before introducing industrial Delta inverse kinematics. The first desktop viewer is 3D and renders a triangular top frame, three vertical actuator rails, moving carriages, parallel links, platform/TCP, reachable workspace, and tool path.

The Drone is a simplified aerial robot model with `DronePose`, `DroneProfile`, `DroneMoveCommand`, `DroneMotionPlanner`, `DroneSimulator`, and `DronePlaybackSampler`. It tracks X/Y/Z position in millimeters plus roll, pitch, and yaw in degrees. Roll and pitch are bounded by a configurable physical tilt limit, while yaw is normalized as continuous heading. It deliberately ignores real quadcopter dynamics, thrust, stabilization, PID, and wind so students can first understand 3D pose, attitude, flight-volume limits, and coordinated movement. The desktop viewer applies the simulated attitude to its schematic 3D model and also renders the flight-volume boundary, ground grid, rotor arms, heading indicator, and flight path.

### Hardware

Hardware communication is not part of the first implementation. Future hardware work should live in `RobotStudio.Hardware` and must not leak into `RobotStudio.Domain`.

`RobotStudio.Hardware` currently defines only boundary contracts. `HardwareCommandEnvelope` describes a domain command being prepared for a future hardware adapter, including a command id and timeout. `HardwareCommandResult` describes the result returned by a future adapter. `IHardwareRobotConnection` defines the future send boundary without choosing serial APIs, port discovery, Arduino, ESP32, firmware protocols, or actuator models.

The first planned educational hardware target is an Arduino-compatible controller using stepper motors for the introductory Cartesian prototype. This is metadata only: no serial adapter, firmware protocol, board detection, pin mapping, or motor driver implementation exists yet.

## Open Decisions
