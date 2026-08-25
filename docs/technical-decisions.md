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

Cartesian playback snapshot format version 4 adds exact per-command motion summaries derived from simulation timeline boundaries. Each summary records start and end positions, involved axes, profile shape, effective velocity limit, peak velocity, acceleration, and phase durations. Didactic tooling consumes these summaries instead of estimating command metrics from fixed-interval playback frames, which may not coincide with command boundaries. Versions 1 through 3 remain valid without summaries.

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

Playback snapshots include `PlaybackSnapshotMetadata` with a format version, robot family, distance unit, time unit, and sample interval. Future UI and tooling should check this metadata before consuming snapshot contents so the export format can evolve deliberately. Cartesian format version 3 adds requested movement velocity and wait-duration metadata. Version 4 adds exact command motion summaries while retaining validation compatibility with versions 1 through 3.

`PlaybackSnapshotValidator` validates exported playback snapshots before a UI or external tool consumes them. It checks supported metadata versions, required sections, frame/pose/scene-frame count consistency, non-negative duration, version 2 motion metrics, version 3 command metadata, and version 4 command motion summaries.

### Scripting

Script compilation is exposed through `IRobotScriptDialect`. A dialect receives script text and produces a `RobotScriptCompilation` containing ordered source statements and the shared executable `RobotCommandSequence`. This preserves non-executable directives for didactic tooling while keeping simulation and domain validation independent of source syntax. `Parse` remains a convenience operation for consumers that need only commands. `RobotScriptParser` implements the Simple DSL for every available robot family. `GCodeParser` implements the first introductory Cartesian subset, and both compilers produce the same `HomeCommand`, `MoveToCommand`, and `WaitCommand` types.

Initial target syntax:

```txt
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
HOME
```

The first G-code subset supports `G28`, `G1`, `G4`, `G21`, `G90`, and `G91`. Optional `F` follows the conventional millimeters-per-minute unit and is converted to the domain's millimeters-per-second velocity. `G4 P` uses milliseconds. `G21` records the project's fixed millimeter unit, while `G20` is rejected explicitly. Feed rate is intentionally not retained as modal state in this introductory subset. `GCodeWriter` emits `G21` and `G90` as a deterministic preamble.

G-code parsing and robot mapping are separate stages. `GCodeParser.CompileProgram` produces a `GCodeProgram` made of tool-space instructions without requiring a robot position. `IGCodeCommandMapper` translates that semantic program into the shared `RobotScriptCompilation`. Cartesian, SCARA, and Simple Arm mappers independently resolve positioning state and produce commands appropriate to their simulation contracts. The existing parameterless `Compile` and `Parse` methods retain their Cartesian behavior by delegating to `CartesianGCodeCommandMapper`, so current CLI and Desktop consumers remain compatible.

`RobotScriptParseContext` carries an `IRobotPosition`, so the shared dialect boundary does not depend on a Cartesian type. Each mapper must explicitly validate that the supplied position belongs to a compatible robot family. The Cartesian mapper accepts `CartesianPosition`, tracks positioning mode and the last resolved position only while mapping one program, and rejects incompatible contexts clearly. In `G90`, omitted axes retain their current values; in `G91`, supplied coordinates are displacements and omitted axes contribute zero. Every Cartesian `G1` is emitted as an absolute `MoveToCommand`, ensuring Domain, Motion, Simulation, CLI, and Desktop do not need G-code-specific movement rules. `G28` updates the mapper's known position to the origin without changing the selected positioning mode. `G21`, `G90`, and `G91` remain available as typed non-executable statements.

Cartesian simulation steps carry requested velocity and wait duration from the compiled command through samples, visual states, poses, and scene frames. Desktop charts and explanations consume this metadata directly. The UI must not reparse individual source lines because modal languages require the complete preceding program state for correct interpretation.

The standard RobotStudio mapping policy defines G-code as TCP tool-space motion. Coordinate words never alias joint numbers, wheel commands, or parallel actuators. Cartesian Robot and XY Plotter mappings are available because their tool coordinates map directly to linear axes. SCARA maps planar `G1 X/Y` to `ScaraLinearMoveCommand`; `ScaraCartesianMotionPlanner` samples the requested line at no more than 2 mm intervals, applies deterministic elbow-down inverse kinematics at every waypoint, and derives safe TCP velocity and acceleration limits from the participating joints. One global trapezoidal TCP profile controls the complete command, so internal kinematic samples do not introduce artificial stops. Existing `ScaraMoveJointsCommand` remains available for explicit joint-space lessons.

Simple Articulated Arm maps planar `G1 X/Y/A` poses through deterministic positive-bend inverse kinematics. `A` is the tool orientation in degrees; `Z` must remain zero and `B/C` are rejected because the current model is planar. Its planner samples both translation and orientation, validates the resulting joint configurations and collision path, and uses one normalized progress profile constrained by joint velocity and acceleration.

Delta Robot maps `G1 X/Y/Z` through an exact inverse of its introductory linear parallel-actuator model. A straight TCP line therefore produces synchronized straight actuator movement without numerical IK or configuration selection. The planner samples that path for physical-limit and collision validation, derives TCP velocity and acceleration limits from actuator ratios, and uses one global trapezoidal profile. Orientation words are rejected because the current Delta model controls position only.

The 6-DOF Industrial Arm maps `G1 X/Y/Z/A/B/C` to a full TCP pose: `A`, `B`, and `C` represent roll, pitch, and yaw rather than joints. A deterministic positive-elbow/wrist-neutral configuration makes inverse kinematics and playback repeatable. The introductory serial-chain model couples TCP yaw to the J1 base azimuth, so `C` must agree with the azimuth of `X/Y`; incompatible poses are rejected instead of silently changing orientation. The Cartesian planner samples translation and shortest-path orientation together, resolves each sample through inverse kinematics, and derives one acceleration-aware progress profile from all joint limits. Differential Drive and Drone mappings are intentionally not applicable because CNC G-code does not express their movement models clearly. `GCodeRobotMappingCatalog` exposes this compatibility policy without adding robot-specific branches to the parser.

`GCodeWriter` converts supported domain command sequences into this subset. It is used to present equivalent desktop teaching examples without duplicating simulation rules.

The desktop's optional G-code guide is a presentation-only interpretation built from source lines and `GCodeRobotMappingCatalog` metadata. It does not modify scripts, execute commands, or replace parser validation. A shared control presents the same line-by-line concepts in every G-code-capable workspace while adapting coordinate explanations to each robot mapping.

`RobotScriptDialectResolver` centralizes command-line dialect selection. An explicit `dsl` or `gcode` request takes precedence; otherwise `.gcode` selects G-code and all other extensions retain the backward-compatible Simple DSL default. The resolver returns `IRobotScriptDialect`, so the CLI's validation, simulation, playback, and export paths remain independent of concrete parser classes.

### UI And Visualization

The first desktop UI uses WPF and targets Windows. This keeps the first visual iteration package-free and focused on rendering the simulation contract already produced by `RobotStudio.Simulation`.

`MainWindow` remains the WPF composition root, but its code-behind is organized as one partial class per cohesive responsibility: shell lifecycle, glossary, scripting, configuration, active commands and recovery, interactions, catalog and viewer configuration, playback, charts, family rendering, script infrastructure, and Cartesian rendering. `MainWindow.xaml.cs` owns only shared state, initialization, keyboard routing, and top-level playback/reset behavior. Shared brushes, styles, and non-behavioral templates live in `Styles/MainWindowStyles.xaml`. New behavior should be added to the matching partial file or a dedicated control/service instead of growing the root code-behind again.

This partial-class split improves navigation and reviewability without pretending to remove the window's shared-state coupling. A future desktop architecture pass should extract each robot workspace into a dedicated control and presenter before adding substantially more workspace behavior. An architecture test currently limits production C# files to 1,000 lines so another oversized file is detected during normal test execution.

The full solution is validated on Windows in CI. Earlier cross-platform validation was removed because the product and release tooling currently depend on Windows desktop technology.

Architecture tests live in `RobotStudio.Architecture.Tests`. They read project files as XML and guard the allowed project reference map, the purity of `RobotStudio.Domain`, and the WPF-only desktop boundary. These tests are intentionally package-free so the architecture rules remain easy to inspect in class.

The desktop viewer must consume simulation output, especially scene frames and viewport data, instead of duplicating simulation, motion, or geometry rules.

Cross-family simulation contracts belong in `RobotStudio.Simulation`. `IRobotPlaybackFrame`, `IRobotPlaybackSnapshot`, and `IRobotPlaybackSnapshot<TFrame>` expose shared timeline metadata such as time, state, command source, duration, frame count, and success/failure without forcing Cartesian positions, mobile poses, or articulated joints into one shape. Family-specific snapshots keep their strongly typed frame lists, while shared tools can use `RobotPlaybackSummary` when only high-level playback information is needed.

Visual coordinate and camera decisions must remain outside the domain model.

Reusable viewport infrastructure belongs in `RobotStudio.Desktop.Rendering`. Orbit camera construction, angular normalization, pointer-drag interaction state, scene lighting, and basic mesh generation are desktop rendering concerns, not domain or simulation rules. Robot-specific viewers should decide which conceptual parts to show, while shared rendering helpers should handle repeated WPF `Viewport3D` mechanics such as cameras, lights, mouse capture, drag deltas, boxes, oriented links, planar grids, reachable workspace rings, and simple volumetric markers.

Mechanical-showcase cameras derive their initial target and distance from the imported scene bounds instead of fixed model dimensions. Left-button dragging orbits, middle-button or `Shift` + left-button dragging pans in the camera plane, `Ctrl` + mouse wheel zooms, and reset restores the model-derived framing. The interaction math remains independent from HelixToolkit scene objects so it can be tested without a live renderer.

Milestone 6 adds a realistic mechanical-showcase path without replacing the current schematic renderer or deterministic simulation workspace. The schematic workspace remains the executable environment for scripts, commands, planning, and playback. The showcase prioritizes part identification, mechanical relationships, and short predefined demonstrations. A presentation-only demonstration controller may drive renderer-neutral component poses, but rendered meshes must never become the source of domain or simulation state. Graphics-library objects, asset formats, cameras, materials, lighting, hit testing, and rendering frame timing remain desktop concerns.

Available robot cards expose `Open Simulator` and `Explore Mechanics` as distinct initial entry points. Realistic assets use stylized technical realism and are original generic RobotStudio teaching models rather than photorealistic or branded industrial reproductions. Mechanical credibility and component readability take priority over fine visual detail. The showcase supports curated camera and component-inspection interactions, predefined demonstrations, and model-specific transparency, cutaway, or exploded teaching views; it is not a geometry editor or arbitrary animation environment.

Future rendering dependencies must be open source and usable without paid licenses, subscriptions, royalties, or commercial licensing fees. HelixToolkit is the preferred candidate, Stride is the advanced alternative, and Veldrid is a low-level fallback rather than the preferred path. Ab4d.SharpEngine is rejected as the default because its licensing model can introduce commercial licensing requirements. The evaluation must be repeated at the start of Milestone 6 before a dependency is added. glTF 2.0/GLB is the preferred future model asset direction. The complete milestone specification is in [Advanced 3D Visualization](advanced-3d-visualization.md).

The Milestone 6 revalidation selected stable `HelixToolkit.Wpf.SharpDX` 3.1.2 for an isolated Cartesian proof of concept. The matching `HelixToolkit.SharpDX.Assimp` 3.1.2 integration is isolated in the desktop asset importer and does not enter portable projects. Approval remains provisional until combined WPF runtime, PBR material, authored-model loading, hit-testing, transparency or cutaway, retained-transform, and disposal tests pass. Blender is the preferred offline authoring tool, and exported GLB assets must pass the Khronos glTF Validator plus RobotStudio semantic validation. The full evaluation is in [Renderer And Asset Pipeline Evaluation](renderer-evaluation.md).

`RobotStudio.Visualization` owns renderer-neutral semantic part identifiers, hierarchical visual-model definitions, component poses, curated demonstration keyframes, sampling, and transform resolution. It has no project, package, Windows, WPF, or HelixToolkit dependencies. `RobotStudio.Desktop` adapts those contracts to the selected renderer and is the only project permitted to reference HelixToolkit. Catalog metadata advertises a mechanical showcase independently from a robot's schematic viewer descriptor.

Visual asset packages use a minimal version 1 JSON manifest beside a packaged GLB. The portable visualization project owns parsing and semantic validation; the desktop project owns file discovery and package caching. A manifest maps one or more named GLB nodes to stable `RobotPartId` values and must cover every selectable part in the corresponding `RobotVisualModelDefinition`. It intentionally excludes materials, cameras, demonstrations, renderer types, and transform behavior so those concerns do not become an accidental asset schema. Details are recorded in [Visual Asset Manifest](visual-asset-manifest.md).

The desktop GLB importer binds each explicitly named scene node and its descendants to the mapped `RobotPartId`. A nested explicit mapping starts a different semantic subtree. This supports logical assemblies whose exported transform node contains several mesh children while ensuring hit testing resolves to RobotStudio concepts instead of raw asset names. Missing referenced nodes and duplicate referenced names fail deterministically before a scene reaches the viewport.

The first Cartesian package is an original technical GLB stored with the application and generated reproducibly by `tools/RobotStudio.VisualAssetBuilder`. The utility runs only during asset development; it is not referenced by the application and does not become a rendering or simulation dependency. Its small shared box/cylinder mesh library is sufficient for validating the complete package pipeline and semantic hierarchy. Blender remains the preferred authoring tool when later robot assets require detailed mechanical meshes, UVs, or textures.

Mechanical teaching views must change the appearance of one retained imported hierarchy instead of loading duplicate models for assembled, cutaway, or highlighted presentations. The Cartesian drive-system view derives ghosting and technical colors from `RobotPartKind`; selected ghosted structures remain translucent so inspection does not close the cutaway. The procedural scene is retained only as a deterministic fallback when the packaged asset cannot be loaded.

Mechanical showcases should prefer recognizable, unbranded, real-world machine arrangements when they clarify how a robot family is applied. The first Cartesian showcase therefore uses a desktop-machine arrangement inspired by common FDM printers: a Y-moving work platform, an X-moving process tool, and a dual-column synchronized Z gantry. This visual embodiment does not add printing-specific domain behavior such as extrusion, thermal control, slicing, or layer deposition; those concerns require a distinct future template if implemented.

Local example metadata belongs in `RobotStudio.Desktop.Examples`. Examples are product/UI teaching assets that provide starter scripts for available viewers. They should not be hard-coded inside individual event handlers, and they should remain separate from parser or simulator rules. `RobotExampleExpectedResult` distinguishes executable lessons from intentional validation failures so automated tests do not mistake a negative lesson for a broken example. Standalone files under `examples/` are grouped by robot model; tests keep focused Cartesian teaching files synchronized with their desktop catalog entries.

Shared viewer presentation logic belongs in `RobotStudio.Desktop.Viewers`. The WPF window may still own concrete control events and drawing calls, but repeated formatting of playback state, frame counters, command names, footer text, and didactic explanations should be moved into small presenter types that can be tested without launching WPF.

Desktop script validation messages are formatted in `RobotStudio.Desktop.Scripting`. The parser and domain still throw explicit technical exceptions, while the desktop layer translates them into concise student-facing summaries with a probable category, the original detail, and a suggested next action.

The desktop start screen uses a didactic robot catalog made of family descriptors, template descriptors, availability status, capabilities, viewer descriptors, and complexity levels. This catalog is product metadata for navigation and learning progression; it does not implement robot simulation behavior.

Desktop viewers share reusable templates for script actions, playback actions, and contextual fault recovery. Non-Cartesian viewers also share a `ViewerTimeline` control. Recovery actions remain hidden during normal operation and are presented in the script panel only while the retained session is faulted.

Cartesian profile editing belongs in the Desktop as text-input orchestration, while physical validity remains enforced by Domain `Axis` and `CartesianRobotProfile` objects. `CartesianProfileInput` performs invariant, finite-number conversion without referencing WPF controls. Applying a profile resets the Cartesian simulation start to the fixed HOME origin and regenerates all playback-derived visualization. A valid profile is retained even when the current script becomes invalid; in that case Desktop replaces stale playback with a HOME preview and keeps the script available for correction. Profile persistence is a separate future concern and must not add file access to Domain.

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

The 6-DOF Industrial Arm uses explicit `J1` through `J6` joint coordinates, individual limits, and coordinated joint-space planning. Its introductory kinematic model represents base yaw, shoulder/elbow/wrist pitch, and wrist/tool roll as a simplified serial chain. Forward kinematics supports joint-space lessons, while one explicit deterministic inverse configuration supports linear G-code tool-pose lessons without claiming to model every industrial-arm solution, singularity, or wrist topology. Its 3D viewer renders the chain, joint markers, TCP orientation, reachable floor area, path playback, state, and examples from either dialect while consuming the same simulation snapshot.

The Delta Robot starts as a simplified parallel robot model with `DeltaActuatorPosition`, `DeltaRobotProfile`, `DeltaKinematics`, `DeltaMoveActuatorsCommand`, `DeltaMotionPlanner`, `DeltaSimulator`, and `DeltaPlaybackSampler`. The initial educational model uses three vertical actuators named A/B/C and maps actuator differences to X/Y tool displacement while the actuator average drives Z. This deliberately teaches parallel coupling before introducing industrial Delta inverse kinematics. The first desktop viewer is 3D and renders a triangular top frame, three vertical actuator rails, moving carriages, parallel links, platform/TCP, reachable workspace, and tool path.

The Drone is a simplified aerial robot model with `DronePose`, `DroneProfile`, `DroneMoveCommand`, `DroneMotionPlanner`, `DroneSimulator`, and `DronePlaybackSampler`. It tracks X/Y/Z position in millimeters plus roll, pitch, and yaw in degrees. Roll and pitch are bounded by a configurable physical tilt limit, while yaw is normalized as continuous heading. It deliberately ignores real quadcopter dynamics, thrust, stabilization, PID, and wind so students can first understand 3D pose, attitude, flight-volume limits, and coordinated movement. The desktop viewer applies the simulated attitude to its schematic 3D model and also renders the flight-volume boundary, ground grid, rotor arms, heading indicator, and flight path.

### Hardware

Hardware communication is not part of the first implementation. Future hardware work should live in `RobotStudio.Hardware` and must not leak into `RobotStudio.Domain`.

`RobotStudio.Hardware` currently defines only boundary contracts. `HardwareCommandEnvelope` describes a domain command being prepared for a future hardware adapter, including a command id and timeout. `HardwareCommandResult` describes the result returned by a future adapter. `IHardwareRobotConnection` defines the future send boundary without choosing serial APIs, port discovery, Arduino, ESP32, firmware protocols, or actuator models.

The first planned educational hardware target is an Arduino-compatible controller using stepper motors for the introductory Cartesian prototype. This is metadata only: no serial adapter, firmware protocol, board detection, pin mapping, or motor driver implementation exists yet.

## Open Decisions
