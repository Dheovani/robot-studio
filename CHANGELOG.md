# Changelog

All notable user-facing changes to RobotStudio are documented in this file.

## Unreleased

### Added

- Added a time-based desktop render timeline that keeps WPF presentation cadence independent from deterministic simulation sampling and playback duration.
- Added consistent primary, secondary, and ghost desktop button roles with distinct navigation, playback, and command hierarchy.
- Added a cross-family rendering smoke gate covering assets, semantic selection, teaching modes, demonstrations, and transforms for all eight mechanical showcases.
- Added dedicated schematic scene composers for every implemented robot simulator, isolating cameras, overlays, workspaces, trajectories, coordinate mapping, and robot geometry from the desktop window.
- Added an immutable 2D canvas scene contract and WPF presenter for the Differential Drive simulator.
- Added reusable dark-theme dropdown and compact scrollbar styles across the catalog, simulators, glossary, editors, and mechanical showcases.
- Added a compact right-edge navigation rail for the Cartesian and XY Plotter sidebars, separating Script, Control, Monitor, and View workflows while preserving each area's scroll position.
- Added automatic debounced script validation with compact status feedback and direct active-line selection in the script editor.
- Added runtime interface localization with English and Brazilian Portuguese resources, a catalog language selector, translated robot metadata, and language-neutral DSL/G-code semantics.
- Added an introductory G-code dialect that maps `G28`, `G1`, and `G4` to the same homing, Cartesian movement, and wait domain commands used by the Simple DSL.
- Added a desktop dialect selector for Cartesian and XY Plotter scripts, including G-code-aware examples, manual jogging, direct commands, validation, simulation, editor tags, and automatic dialect selection for `.robot` and `.gcode` files.
- Added equivalent Simple DSL and G-code Cartesian example files for comparing both command languages.
- Added dialect-aware CLI commands that infer Simple DSL or G-code from script extensions, accept an explicit `--dialect` override, and report the selected dialect.
- Added `G90` absolute and `G91` relative positioning, retained coordinates, parse-context resolution, dedicated desktop editor tags, and a relative-motion teaching example.
- Added compiled script statements that preserve positioning-mode directives alongside executable domain commands.
- Added Cartesian playback snapshot format 3 with requested movement velocity and wait-duration metadata.
- Added organized per-robot example files and Cartesian lessons for axis-limit errors, requested versus effective speed, jog-style sequencing, waits, and homing.
- Added an in-memory Cartesian robot configuration panel for editing axis limits, maximum velocity, and maximum acceleration with immediate playback and workspace regeneration.
- Added Cartesian playback snapshot format 4 with exact per-command motion summaries, including profile shape, phase durations, peak velocity, acceleration, and involved axes.
- Added acceleration-aware Cartesian movement explanations for triangular and trapezoidal profiles, limiting axes, phase timing, and the current motion phase.
- Added a searchable in-app robotics glossary with 48 terms, topic filters, related concepts, catalog and workspace access, and a `Ctrl+G` shortcut.
- Added an optional line-by-line G-code guide that follows script edits and explains modal state, robot-specific tool coordinates, feed rate, homing, and dwell commands.
- Added a semantic G-code program model and robot command mapping boundary so syntax, modal state, and robot-specific command generation evolve independently.
- Added an explicit G-code compatibility catalog: Cartesian Robot and XY Plotter mappings are available, articulated and parallel mappings require future tool-path kinematics, and mobile/aerial mappings are intentionally not applicable.
- Added `G21` millimeter declarations and clear rejection of unsupported `G20` inch mode.
- Added SCARA tool-space G-code with planar `G1 X/Y`, deterministic elbow-down inverse kinematics, continuous acceleration-aware linear TCP playback, desktop dialect selection, and local examples.
- Added Simple Articulated Arm tool-pose G-code with planar `G1 X/Y/A`, deterministic positive-bend inverse kinematics, continuous joint-limited playback, desktop dialect selection, and local examples.
- Added Delta Robot tool-space G-code with `G1 X/Y/Z`, exact inverse kinematics, synchronized actuator constraints, continuous linear TCP playback, desktop dialect selection, and local examples.
- Added 6-DOF Industrial Arm tool-pose G-code with `G1 X/Y/Z/A/B/C`, deterministic positive-elbow/wrist-neutral inverse kinematics, joint-constrained linear playback, desktop dialect selection, and local examples.
- Added `G-code` capability badges only to robot cards with an executable mapping.
- Added the first Cartesian mechanical-showcase prototype with a retained 3D scene, curated motion, semantic component selection, highlighting, and educational part inspection alongside the existing schematic simulator.
- Added renderer-neutral visual-model, robot-part hierarchy, component-pose, demonstration, sampling, and transform-resolution contracts in a dedicated visualization project.
- Added assembled and drive-system teaching views to the Cartesian mechanical showcase, using transparency and technical highlighting to expose rails, belts, lead screws, and motors without rebuilding the scene.
- Added a versioned visual-asset package contract with safe local GLB paths, semantic node mappings, deterministic validation errors, and desktop package caching.
- Added an isolated HelixToolkit Assimp GLB importer that maps imported scene hierarchies to semantic robot parts and rejects missing or ambiguous asset nodes.
- Added the first original packaged Cartesian GLB asset with a technical desktop-machine hierarchy, authored PBR materials, semantic component animation, selection, highlighting, and a deterministic development-time asset builder.
- Added model-aware camera framing and viewport pan to the Cartesian mechanical showcase, with middle-button or `Shift` + left-button dragging.
- Added a Cartesian mechanical-showcase motion-axis layer with color-coded X/Y/Z direction guides that remain synchronized with their corresponding moving assemblies.
- Added a selectable Cartesian axis-by-axis mechanical demonstration with independent Y, X, and Z phases and contextual descriptions for each showcase sequence.
- Added a controlled exploded-assembly layer with a staged assembly sequence that joins the Cartesian controller, moving bed, Z gantry, X carriage, and tool while preserving semantic hierarchy and selection.
- Added a data-driven mechanical-showcase catalog that resolves robot cards to validated presentation definitions and creates renderer views on demand.
- Added an original XY Plotter mechanical showcase with a packaged GLB, semantic paper-bed and X/Y drive hierarchy, component inspection, and a procedural fallback scene.
- Added XY Plotter rectangular-path, isolated-axis, and staged-assembly demonstrations together with assembled, drive-system, motion-axis, and exploded layers.
- Added an original round service-robot Differential Drive showcase with independent drive units, encoders, support caster, controller, battery, range sensor, and a packaged semantic GLB.
- Added Differential Drive square-route, bidirectional turning, body-frame, drive-system cutaway, and staged-assembly teaching views.
- Added an original SCARA mechanical showcase with a packaged semantic GLB, shoulder and elbow servo chain, vertical spindle, parallel gripper, selectable components, and a procedural fallback scene.
- Added SCARA pick-and-place, individual-joint, drive-system, joint/tool-axis, and staged-assembly teaching views with pivot-correct articulated motion.
- Added an original Simple Articulated Arm mechanical showcase with a packaged semantic GLB, rotating base, shoulder and elbow drive assemblies, serial structural links, wrist, parallel gripper, selectable components, and a procedural fallback scene.
- Added Simple Articulated Arm reach-and-transfer, individual-joint, drive-system, joint-axis, and staged-assembly teaching views with pivot-correct shoulder and elbow motion.
- Added an original linear Delta Robot mechanical showcase with a packaged semantic GLB, overhead support frame, three servo-driven linear actuators, six constrained links, moving platform, vacuum tool, selectable components, and a procedural fallback scene.
- Added Delta coupled pick-and-place, individual-actuator, drive-system, actuator/TCP-axis, and staged-assembly teaching views with continuously connected parallel links.
- Added reusable parallel-link pose composition that keeps a rendered link attached to two independently moving semantic components throughout interpolated mechanical demonstrations.
- Added an original quadcopter Drone mechanical showcase with a packaged semantic GLB, four visible two-blade propellers, brushless motors, battery, flight controller, IMU, camera, landing gear, selectable components, and a procedural fallback scene.
- Added Drone flight-and-attitude, counter-rotating motor-pair, avionics-and-power, body-axis, and staged-assembly teaching views with propeller rotation inherited through the moving airframe.
- Added an original 6-DOF Industrial Arm mechanical showcase with a floor pedestal, enclosed shoulder and elbow drives, load-bearing links, a three-axis wrist, service routing, parallel gripper, selectable components, and a packaged semantic GLB.
- Added 6-DOF coordinated-pick, wrist-orientation, joint-drive cutaway, six-axis guide, and staged-assembly teaching views with pivot-correct serial motion.
- Generalized the deterministic visual-asset builder to support any single semantic root instead of requiring every robot asset to use a part named `base`.

### Changed

- Standardized simulator and mechanical-showcase headers with neutral playback actions and kept glossary access in the robot catalog instead of every workspace.
- Centralized schematic WPF viewport lifecycle for all six 3D simulators behind a shared scene contract and presenter, including camera, lighting, model-root, and overlay replacement.
- Refined the Drone mechanical model into a recognizable compact quadcopter with a low rounded-rectangle hull, integrated diagonal arms, paired propeller blades, and enclosed central avionics based on a common consumer-drone arrangement.
- Refined the SCARA mechanical model with a compact pedestal, continuous light-colored link covers, a larger wrist housing, a blue vertical spindle, and a metal parallel gripper based on a recognizable industrial SCARA arrangement.
- Defined G-code as TCP tool-space motion rather than direct joint or actuator commands; non-Cartesian families continue using Simple DSL until a compatible path planner and inverse kinematics can preserve that meaning.
- Changed the Cartesian mechanical showcase into a recognizable desktop-machine arrangement with a Y-moving work platform, an X tool carriage, and a synchronized dual-column Z gantry.
- Changed mechanical-showcase fallback geometry, assets, overlays, layers, and initial selection from control-owned Cartesian assumptions into model presentation data ready for additional robot families.
- Changed the Cartesian drive-system teaching view to use the same packaged GLB as the assembled view, with semantic transparency and technical highlighting instead of switching back to procedural geometry.
- Changed generated G-code to include deterministic `G21` millimeter and `G90` absolute-positioning preambles.
- Changed Cartesian charts and movement explanations to consume simulation metadata instead of reparsing isolated script lines.
- Changed Cartesian movement explanations to use exact command boundaries instead of estimating motion from fixed-interval playback frames.

### Fixed

- Fixed the Delta mechanical-showcase controller floating outside the support frame by mounting its enclosure beneath the rear crossbeam with visible brackets.
- Fixed SCARA mechanical links separating between demonstration keyframes by recomputing revolute-joint pivot compensation after rotation interpolation.
- Fixed Cartesian Robot and XY Plotter playback becoming inactive after switching between Simple DSL and G-code.
- Fixed the mechanical showcase header layout and moved playback actions into the same top-toolbar pattern used by simulation workspaces.
- Fixed mechanical-showcase camera navigation with full-viewport drag orbit, bounded elevation, predictable reset, and removal of the unstable view cube.
- Fixed the mechanical showcase opening with empty component details by selecting the generic process tool initially.
- Fixed the Cartesian mechanical showcase front orientation and made its key light follow the orbit camera so inspectable components remain readable from different angles.
- Fixed transparent exterior parts intercepting component selection in the mechanical showcase drive-system layer.

## 1.1.0 - Multi-Robot Teaching Platform Expansion

### Added

- Added a didactic robot catalog organized by learning complexity, with available and planned robot templates.
- Added planned catalog descriptors for the Cylindrical Robot, Ackermann Steering Robot, Omnidirectional Robot, Self-Balancing Robot, Stewart Platform, and Mobile Manipulator.
- Added the XY Plotter as a beginner Cartesian-family model for two-axis drawing and planar command sequencing.
- Added the Differential Drive Robot as the first mobile robot model, including deterministic motion, DSL commands, playback, and a desktop viewer.
- Added the SCARA Robot as an articulated robot model, including joint-space motion, kinematics, DSL commands, playback, and a 3D desktop viewer.
- Added the Simple Articulated Arm as a three-joint arm model, including forward kinematics, DSL commands, playback, and a 3D desktop viewer.
- Added the Delta Robot as the first parallel robot model, including simplified parallel kinematics, actuator-space commands, playback, and a 3D desktop viewer.
- Added the Drone as the first aerial robot model, including 3D pose, yaw orientation, coordinated flight planning, DSL commands, playback, and a 3D desktop viewer.
- Added the 6-DOF Industrial Arm, including six-joint limits, simplified forward kinematics, coordinated joint planning, the `ARM6` DSL command, deterministic simulation, playback sampling, local examples, didactic state presentation, and a 3D desktop viewer.
- Added shared playback snapshot and frame contracts so desktop and future tooling can consume different robot families without forcing them into one position model.
- Added reusable desktop rendering helpers for orbit cameras, mesh primitives, reachable workspaces, paths, and volumetric robot parts.
- Added local desktop teaching examples and selectors for every available robot viewer.
- Added desktop script load/save support for `.robot` and `.txt` files.
- Added clearer script validation messages for syntax errors, unsupported command arguments, and physical limit violations.
- Added keyboard shortcuts for active-viewer script loading, saving, validation, simulation, playback, frame stepping, zoom, and camera reset.
- Added desktop session recovery actions that execute `HOME` or `RESET` from each robot family's preserved final simulation context.
- Added shared script-action, playback-action, recovery-action, and timeline components for the multi-robot desktop workspaces.
- Added architecture tests to guard project dependency rules and keep WPF isolated from portable core projects.
- Added portable solution validation for non-desktop projects, keeping the CLI and core libraries testable outside the Windows desktop target.
- Added Windows release artifact and installer generation groundwork for distributing the desktop application.
- Added reusable trapezoidal and triangular motion profiles with deterministic acceleration, constant-velocity, and deceleration sampling.
- Added Cartesian playback snapshot format 2 with exact motion phase, velocity, and acceleration metrics while retaining format 1 validation compatibility.
- Added Cartesian profile phase, velocity, and acceleration state values plus an acceleration/deceleration chart in the desktop viewer.
- Added explicit angular acceleration limits and synchronized motion profiles for the SCARA, Simple Articulated Arm, and 6-DOF Industrial Arm.
- Added independent linear and angular acceleration limits and motion profiles for the Differential Drive Robot.
- Added synchronized acceleration-aware actuator profiles for the Delta Robot.
- Added synchronized 3D translation and yaw acceleration profiles for the Drone.
- Added ideal Differential Drive odometry with accumulated left/right wheel travel and rotation in playback and didactic state presentation.
- Added Drone roll and pitch limits, commands, synchronized attitude planning, playback, 3D model inclination, and DSL support alongside yaw.
- Added the cross-family `RESET` command for recovering faulted simulations without changing robot pose, joint/actuator state, odometry, or simulated time.
- Added deterministic Cartesian workspace obstacles and segment/AABB collision detection, including didactic collision metadata and obstruction-aware `MOVE` and `HOME` execution.
- Added planar obstacles and exact swept circular-footprint collision detection for Differential Drive movement, with explicit body radius, contact metadata, and obstruction-aware `DRIVE` and `HOME` execution.
- Added deterministic SCARA link collision sampling with explicit link thickness, semantic first/second-link identification, and obstruction-aware joint movement and homing.
- Added spatial collision environments and family-specific safety envelopes for the Simple Articulated Arm, 6-DOF Industrial Arm, Delta mechanism, and Drone body, completing baseline collision coverage for every implemented family.

### Changed

- Expanded the desktop app from a Cartesian-only visual simulator into a multi-robot teaching environment.
- Refined robot catalog cards with compact rectangular metadata tags, clickable available cards, non-interactive planned-release footers, accurate catalog copy, and interaction feedback reserved for openable models.
- Standardized the existing RobotStudio robot icon across the WPF window and executable as the current application identity.
- Kept simulation headers focused on navigation and playback, with fault recovery controls shown contextually only when needed.
- Updated the robot catalog to derive one to six columns from available width, keeping cards readable on smaller windows while using wide and ultrawide displays more efficiently.
- Unified simulation workspaces with robot-specific headers, quieter secondary actions, consistent viewport and sidebar framing, clearer state/script/explanation sections, and a shared timeline treatment.
- Updated Cartesian and XY plotter planning to respect acceleration limits, producing acceleration-aware durations and Cartesian playback interpolation.
- Updated articulated playback to follow acceleration-aware profile progress instead of constant-speed joint interpolation.
- Updated differential-drive playback to complete its planned translation before rotation and to follow each segment's acceleration-aware progress.
- Updated Delta and Drone playback to follow deterministic profile progress instead of constant-speed interpolation.
- Kept G-code, hardware execution, real drone physics, advanced inverse kinematics, and richer 3D graphics as planned future work.

### Fixed

- Fixed architecture tests across Linux, macOS, and concurrent Windows WPF builds by normalizing project reference paths and ignoring SDK-generated temporary WPF projects.

## 1.0.0 - Stable Cartesian Simulation Release

RobotStudio `1.0.0` is the first stable educational release of the project. It provides a complete introductory robotics workflow centered on a Cartesian X/Y/Z robot, while keeping the architecture prepared for future robot families.

This release is suitable for studying the current simulator, running scripts, inspecting deterministic playback data, and using the first desktop 3D viewer.

### What Is Included

- A clean C#/.NET solution organized into domain, motion, simulation, scripting, hardware boundary, CLI, desktop, and test projects.
- A pure domain model for robot concepts, Cartesian positions, robot states, commands, axis limits, and domain validation.
- A simple linear motion planner that estimates distance, effective velocity, duration, involved axes, and speed limiting.
- A deterministic simulation engine for `HOME`, `MOVE`, and `WAIT` commands.
- A beginner-friendly DSL for writing robot command scripts.
- A CLI workflow for validating scripts, running simulations, exporting playback snapshots, and validating exported snapshots.
- A WPF desktop application with robot selection, Cartesian 3D visualization, orbit camera, zoom, playback controls, manual jog controls, command console, overlays, charts, timeline markers, and didactic explanations.
- A robot catalog that lists the current Cartesian robot and planned future robot families without implementing them prematurely.
- Hardware integration contracts prepared for future serial communication, without real Arduino, ESP32, or device execution yet.
- A script dialect boundary prepared for future G-code support, without implementing G-code parsing yet.
- Automated tests covering domain rules, motion planning, scripting, simulation, playback snapshots, desktop metadata, didactic tooltips, and hardware boundary contracts.
- GitHub Actions CI for restore, build, tests, formatting verification, and test result artifacts.
- Project documentation, including technical decisions, use cases, test map, CI behavior, user guide, contribution policy, security policy, code of conduct, license, and roadmap.

### Current Desktop Experience

- Select the available Cartesian robot from the start screen.
- Inspect the robot in a 3D viewport.
- Rotate the camera by dragging with the mouse.
- Zoom with the mouse wheel or camera controls.
- Run the built-in DSL script.
- Edit scripts directly inside the desktop app.
- Validate, simulate, play, pause, reset, step through frames, and scrub playback.
- Operate the robot manually with jog buttons.
- Execute direct DSL commands from the command console.
- Toggle didactic overlays such as workspace, grid, axes, labels, TCP, path, markers, rails, and components.
- Inspect robot state, position, timing, velocity, movement explanation, charts, and script line mapping.

### Not Included Yet

- Additional implemented robot families.
- G-code parsing or execution.
- Real serial communication.
- Arduino or ESP32 firmware.
- Real hardware control from the desktop app.
- Inverse kinematics.
- Drone physics.
- Collision detection.
- Packaging or installer generation.
- A learning management system.

Future work is tracked in `TODO.md`.
