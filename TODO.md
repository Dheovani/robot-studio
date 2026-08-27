# TODO

This file tracks future work after the `1.1.0` multi-robot teaching platform release. Completed release history belongs in `CHANGELOG.md`.

## 1. Release And Distribution

- [x] Create and push the `v1.1.0` Git tag after the release commit passes CI.
- [ ] Acquire a code signing certificate before publishing signed Windows releases.
- [ ] Add Windows ARM64 CLI release artifacts if student machines need them.

## 2. Scripting And G-Code

- [x] Add a script dialect selector to the Cartesian and XY Plotter desktop workspaces.
- [x] Design the first conservative G-code parser dialect with explicit coordinates before introducing modal positioning state.
- [x] Map `G28` to homing behavior.
- [x] Map `G1 X... Y... Z... F...` to linear movement, interpreting `F` as millimeters per minute.
- [x] Map `G4 P...` to wait/dwell behavior, interpreting `P` as milliseconds.
- [x] Allow Simple DSL and G-code scripts to produce the same domain command model.
- [x] Add equivalent `examples/cartesian/basic.robot` and `examples/cartesian/basic.gcode` programs.
- [x] Add dialect-aware CLI validation, simulation, playback, export, and examples with extension inference and an explicit `--dialect` override.
- [x] Add `G90` absolute and `G91` relative positioning with parse context, retained coordinates, and deterministic conversion to absolute `MoveToCommand` targets.
- [x] Replace the Cartesian-specific shared parse context with an extensible robot-position context and explicit compatibility validation.
- [x] Preserve executable commands and non-executable modal directives in a compiled script representation.
- [x] Carry requested-motion metadata through Cartesian simulation and playback so the desktop does not reparse isolated source lines.
- [x] Define the standard robot mapping policy: G-code describes TCP tool-space motion, each non-Cartesian mapping requires continuous Cartesian planning plus inverse kinematics, and mobile/aerial families retain robot-appropriate DSL commands.
- [x] Add SCARA G-code with planar `G1 X/Y`, deterministic elbow-down inverse kinematics, sampled path validation, one continuous TCP motion profile, simulation playback, desktop dialect selection, examples, and catalog capability metadata.
- [x] Add Simple Articulated Arm G-code with planar `G1 X/Y/A`, deterministic positive-bend inverse kinematics, continuous tool-pose planning and playback, desktop dialect selection, examples, and catalog capability metadata.
- [x] Add Delta Robot G-code with `G1 X/Y/Z`, exact inverse kinematics, synchronized parallel-actuator constraints, continuous linear TCP playback, desktop dialect selection, examples, and catalog capability metadata.
- [x] Add 6-DOF Industrial Arm G-code with `G1 X/Y/Z/A/B/C`, deterministic positive-elbow/wrist-neutral inverse kinematics, continuous full-pose planning and playback, desktop dialect selection, examples, and catalog capability metadata.
- [x] Keep Differential Drive and Drone G-code unavailable and omit the capability badge because CNC tool-space semantics do not fit their motion models.

## 3. Didactic Tools

The Milestone 2 mapping gate is complete: every applicable implemented family has an executable mapping, while Differential Drive and Drone are explicitly non-applicable.

- [x] Expand movement explanations for acceleration-aware plans.
- [x] Add lesson-friendly examples for invalid axis limits.
- [x] Add examples for requested speed versus effective speed.
- [x] Add examples for waits, homing, manual jogging, and command sequencing.
- [x] Add glossary entries for technical robotics terms.
- [x] Add optional line-by-line explanations for supported G-code commands, modal state, coordinates, feed rate, homing, and dwell behavior.

## 4. Desktop UI And Visual Polish

- [x] Replace native desktop select controls with a reusable themed dropdown component that matches the dark RobotStudio visual language.
- [x] Apply the themed dropdown consistently to dialect, examples, language, glossary topics, playback speed, visualization layers, demonstrations, and future selectors.
- [x] Replace native light scrollbars with compact themed scrollbars across sidebars, dialogs, catalog views, editors, and other scrollable panels.
- [x] Define consistent primary, secondary, and ghost button styles so secondary actions do not visually compete with main workflow actions.
- [x] Standardize control heights, spacing, padding, corner radii, borders, and focus states across desktop UI components.
- [ ] Reduce excessive bordered containers and use spacing, background levels, and typography to establish visual hierarchy.
- [x] Define consistent typography and contrast levels for titles, labels, metadata, descriptions, status text, and secondary information.
- [x] Standardize toolbar and panel-header presentation across simulator, catalog, glossary, and mechanics views.
- [x] Refine the Robotics Glossary dialog with consistent themed search, filter, close, scrolling, and result-card components.
- [ ] Refine simulator chrome around the 3D viewport, including toolbar hierarchy, side-panel organization, and playback controls, without changing the renderer itself.
- [x] Replace verbose script validation and playback messages with compact contextual status indicators.
- [x] Highlight the active script line directly in the editor instead of duplicating it in a separate status block.
- [x] Move low-priority script actions such as load, save, and example loading into a compact toolbar or overflow menu where appropriate.
- [x] Introduce shared desktop UI components for buttons, dropdowns, text inputs, icon buttons, badges, panels, toolbars, dialogs, and status indicators.
- [x] Centralize desktop design tokens for colors, spacing, typography, borders, radii, and interaction states to keep future views visually consistent.

## 5. Advanced 3D Visualization And Realistic Robot Rendering

- [x] Revalidate renderer capabilities, maintenance status, platform support, licenses, and asset tooling; select stable `HelixToolkit.Wpf.SharpDX` 3.1.2 provisionally for the isolated WPF proof of concept.
- [x] Define the product boundary and initial catalog navigation between `Open Simulator` and `Explore Mechanics`; evaluate an internal switch only after the separate experiences are usable.
- [x] Define renderer-neutral component-pose, semantic part identifier, hierarchical robot visual-model, demonstration, sampling, and transform-resolution contracts in `RobotStudio.Visualization` without graphics-library types.
- [x] Define and test a presentation-only keyframe demonstration controller for fixed mechanical animations without making rendered meshes the source of robot state.
- [x] Extract schematic WPF viewport lifecycle, camera replacement, lighting, model-root composition, and overlay replacement from `MainWindow` behind a shared desktop presenter contract.
- [x] Move SCARA schematic camera, workspace, path, and robot geometry into a dedicated scene composer.
- [x] Move Simple Articulated Arm schematic camera, workspace, path, and robot geometry into a dedicated scene composer.
- [x] Move Delta Robot schematic camera, workspace, path, and robot geometry into a dedicated scene composer.
- [x] Move Drone schematic camera, workspace, path, and robot geometry into a dedicated scene composer.
- [x] Move 6-DOF Industrial Arm schematic camera, workspace, path, and robot geometry into a dedicated scene composer.
- [x] Move Cartesian and XY Plotter schematic geometry and overlays into dedicated scene composers.
- [x] Move Differential Drive Robot 2D workspace, path, coordinate mapping, and robot drawing into a dedicated scene composer.
- [x] Define and test a minimal version 1 asset manifest that maps glTF 2.0/GLB nodes to RobotStudio semantic parts without placing materials, animation, or renderer concerns in the contract.
- [x] Connect the Assimp GLB importer and semantic scene-node binder to the Cartesian showcase with an original packaged asset, retained semantic transforms, resource reuse, and explicit scene disposal.
- [x] Implement a realistic renderer proof of concept for one existing robot while preserving the current schematic/didactic renderer.
- [x] Add the first mechanical component inspector with names, kinds, functions, motion descriptions, semantic selection, and highlighting for the Cartesian proof-of-concept scene.
- [x] Add initial visualization mode selection for the Cartesian vertical slice through separate schematic/showcase catalog actions and an optional realistic motion-axis overlay layer.
- [x] Add semantic component selection, highlighting, and educational inspection backed by RobotStudio part identifiers rather than raw mesh identifiers.
- [x] Compose axes, workspace, trajectory, coordinate systems, labels, limits, and future collision bounds independently of the selected robot renderer.
  - [x] Define renderer-independent overlay primitives and semantic kinds in `RobotStudio.Visualization`, including a reserved collision-bounds kind.
  - [x] Move Cartesian and XY Plotter grid, coordinate axes, labels, workspace, trajectory, start/end positions, and physical-limit anchors into a renderer-independent overlay composer.
  - [x] Route mechanical-showcase motion-axis guides through the shared overlay scene before Helix conversion.
  - [x] Migrate workspace, path, coordinate, label, and limit overlays for the remaining schematic robot families to the shared contract.
- [x] Separate deterministic simulation ticks from rendering frames and define interpolation, transform-update, and scene-update policies.
- [ ] Measure model loading, rendering, transform updates, and hit-testing performance on representative teaching hardware.
  - [x] Add a repeatable Release diagnostics tool that profiles all eight GLB showcases in live Helix viewports.
  - [x] Record a development-workstation baseline for import, scene preparation, frame cadence, transform updates, and semantic hit testing.
  - [x] Define a tested minimum acceptance budget and make the diagnostics command fail when a robot exceeds it.
  - [x] Make mechanical viewport teardown explicit and verify scene and renderer resource disposal for every profiled robot.
  - [ ] Repeat the baseline on the intended lower-spec teaching hardware and document any required asset or rendering budget changes.
- [x] Add architecture, asset-contract, mode-switching, selection-mapping, and rendering smoke tests before expanding realistic assets to additional robot families.
- [x] Deliver an original realistic mechanical showcase for Cartesian Robot.
- [x] Deliver an original realistic mechanical showcase for XY Plotter.
- [x] Deliver an original realistic mechanical showcase for Differential Drive Robot.
- [x] Deliver an original realistic mechanical showcase for SCARA Robot.
- [x] Deliver an original realistic mechanical showcase for Simple Articulated Arm.
- [x] Deliver an original realistic mechanical showcase for Delta Robot.
- [x] Deliver an original realistic mechanical showcase for Drone.
- [x] Deliver an original realistic mechanical showcase for 6-DOF Industrial Arm.
- [ ] Treat Milestone 5 completion as capability-based rather than deadline-based; do not release until the selected Milestone 2 through 5 scope is complete.

## 6. Future Robot Family Expansion

- [ ] Implement the Cylindrical Robot as the first mixed revolute/prismatic teaching model.
- [ ] Implement the Ackermann Steering Robot for car-like steering geometry and non-holonomic motion.
- [ ] Implement the Omnidirectional Robot for holonomic movement and wheel-speed decomposition.
- [ ] Implement the Self-Balancing Robot after dynamic simulation, sensors, and feedback-control infrastructure exist.
- [ ] Implement the Stewart Platform as the advanced six-actuator parallel mechanism.
- [ ] Implement the Mobile Manipulator as a capstone that coordinates a mobile base and articulated arm.
- [ ] Expand the catalog by mapping new robots for implementation.
- [ ] Add tests before each new robot family is considered complete.

## 7. Hardware Integration

- [ ] Define the first serial protocol draft.
- [ ] Choose the first supported educational controller board.
- [ ] Choose the first supported motor driver setup.
- [ ] Implement serial port discovery.
- [ ] Implement connection open, close, and health checks.
- [ ] Implement command transmission in dry-run mode before enabling real motion.
- [ ] Add hardware safety limits before any real execution path.
- [ ] Add Arduino or ESP32 firmware examples only after the protocol is stable.

## 8. Testing And Quality

- [x] Split the oversized WPF `MainWindow` code-behind into cohesive partial files and extract shared non-behavioral window resources.
- [ ] Extract robot workspaces from `MainWindow.xaml` into dedicated controls and presenters when the next desktop architecture pass begins.
- [ ] Add code coverage reporting when coverage goals are defined.
- [ ] Add stricter analyzers when the coding standard becomes more mature.
- [ ] Add package vulnerability scanning when external dependencies become more relevant.
- [ ] Add UI smoke tests if the desktop workflow becomes stable enough to automate.
