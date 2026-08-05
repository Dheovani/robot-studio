# TODO

This file tracks future work after the first stable Cartesian simulation release. Completed release history belongs in `CHANGELOG.md`.

## 1. Release And Distribution

- [x] Create the `v1.0.0` Git tag when the release is approved.
- [ ] Acquire a code signing certificate before publishing signed Windows releases.
- [ ] Add ARM64 CLI release artifacts if student machines need them.
- [ ] Evaluate a future cross-platform desktop UI strategy after the Windows desktop release.

## 2. Additional Robot Families

- [x] Implement the XY Plotter as the next Cartesian-family teaching model.
- [X] Implement the Differential Drive Robot as the first mobile robot model.
- [x] Implement the SCARA Robot as the first articulated planar robot model.
- [x] Implement the Simple Articulated Arm as the first joint-based arm model.
- [x] Implement the Delta Robot as the first parallel robot model.
- [x] Implement the Drone as the first aerial robot model.
- [ ] Implement the 6-DOF Industrial Arm as the advanced articulated model.
- [x] Define shared contracts that can support Cartesian, mobile, articulated, parallel, and aerial robots without forcing one motion model onto all of them.
- [ ] Expand the catalog by mapping new robots for implementation.
- [ ] Add tests before each new robot family is considered complete.

## 3. Motion And Simulation

- [ ] Add acceleration-aware motion planning.
- [ ] Add optional trapezoidal velocity profiles.
- [ ] Add collision or workspace obstruction concepts when the lessons need them.
- [x] Add joint-space simulation for articulated robots.
- [ ] Add odometry simulation for mobile robots.
- [ ] Add attitude/orientation simulation for aerial robots.
- [ ] Add a clearer recovery workflow for faulted simulations.
- [ ] Add deterministic simulation fixtures for every future robot family.

## 4. Scripting And G-Code

- [ ] Keep the current DSL as the beginner teaching language.
- [ ] Add a script dialect selector to the desktop app.
- [ ] Design the first G-code parser dialect.
- [ ] Map `G28` to homing behavior.
- [ ] Map `G1` to linear movement behavior.
- [ ] Map `G4` to wait/dwell behavior.
- [ ] Allow DSL and G-code scripts to produce the same domain command model where possible.
- [ ] Add examples that compare DSL and G-code versions of the same movement.

## 5. Hardware Integration

- [ ] Define the first serial protocol draft.
- [ ] Choose the first supported educational controller board.
- [ ] Choose the first supported motor driver setup.
- [ ] Implement serial port discovery.
- [ ] Implement connection open, close, and health checks.
- [ ] Implement command transmission in dry-run mode before enabling real motion.
- [ ] Add hardware safety limits before any real execution path.
- [ ] Add Arduino or ESP32 firmware examples only after the protocol is stable.

## 6. Desktop User Experience

- [ ] Improve the visual design of the robot selection screen.
- [ ] Improve the visual design of the Cartesian simulator workspace.
- [x] Improve viewport drag behavior so orbiting works when dragging anywhere inside the simulation area, not only when the pointer starts over a rendered primitive.
- [ ] Add a more polished application logo and brand system if needed.
- [ ] Add a view cube or compact camera orientation selector.
- [ ] Add optional beginner, teacher, and debug display modes.
- [x] Expand the local example catalog into a multi-example gallery for common scripts.
- [ ] Extract a common desktop viewer shell for repeated script panels, timeline controls, and playback controls.
- [x] Extract shared non-Cartesian frame presentation for state panels and didactic movement explanations.
- [x] Add import/export for desktop scripts.
- [x] Add clearer validation summaries for invalid commands.
- [x] Add keyboard shortcuts for playback, script files, validation, simulation, frame stepping, zoom, and camera controls.

## 7. Didactic Tools

- [ ] Expand movement explanations for acceleration-aware plans.
- [ ] Add lesson-friendly examples for invalid axis limits.
- [ ] Add examples for requested speed versus effective speed.
- [ ] Add examples for waits, homing, manual jogging, and command sequencing.
- [ ] Add glossary entries for technical robotics terms.
- [ ] Add optional inline explanations for future G-code commands.

## 8. Testing And Quality

- [ ] Add code coverage reporting when coverage goals are defined.
- [ ] Add stricter analyzers when the coding standard becomes more mature.
- [ ] Add package vulnerability scanning when external dependencies become more relevant.
- [ ] Add UI smoke tests if the desktop workflow becomes stable enough to automate.
- [ ] Add snapshot compatibility tests before changing playback JSON contracts.
- [x] Add architecture tests to guard project dependency rules.

## 9. Future 3D Visualization Quality

- [ ] Replace the current simplified 3D robot graphics with richer didactic visuals for mechanisms, frames, joints, links, tools, rotors, and workspaces.
- [ ] Add more advanced 3D rendering primitives and scene composition helpers before implementing highly detailed robot models.
- [ ] Improve visual fidelity while keeping the graphics educational rather than CAD-realistic.
