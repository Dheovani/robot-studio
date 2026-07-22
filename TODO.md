# TODO

This file tracks future work after the first stable Cartesian simulation release. Completed release history belongs in `CHANGELOG.md`.

## 1. Release And Distribution

- [ ] Review the `1.0.0` release documentation before tagging.
- [ ] Create the `v1.0.0` Git tag when the release is approved.
- [ ] Decide whether the first distributed build will be a zipped folder, installer, or self-contained executable.
- [ ] Add a repeatable Release build command for the desktop app.
- [ ] Add release artifact generation to GitHub Actions.
- [ ] Add desktop screenshots to `README.md`.
- [ ] Add a short animated preview or playback GIF when the UI stabilizes visually.

## 2. Additional Robot Families

- [ ] Implement the XY Plotter as the next Cartesian-family teaching model.
- [ ] Implement the Differential Drive Robot as the first mobile robot model.
- [ ] Implement the SCARA Robot as the first articulated planar robot model.
- [ ] Implement the Simple Articulated Arm as the first joint-based arm model.
- [ ] Implement the Delta Robot as the first parallel robot model.
- [ ] Implement the Drone as the first aerial robot model.
- [ ] Implement the 6-DOF Industrial Arm as the advanced articulated model.
- [ ] Define shared contracts that can support Cartesian, mobile, articulated, parallel, and aerial robots without forcing one motion model onto all of them.
- [ ] Add tests before each new robot family is considered complete.

## 3. Motion And Simulation

- [ ] Add acceleration-aware motion planning.
- [ ] Add optional trapezoidal velocity profiles.
- [ ] Add collision or workspace obstruction concepts when the lessons need them.
- [ ] Add joint-space simulation for articulated robots.
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
- [ ] Add a more polished application logo and brand system if needed.
- [ ] Add a view cube or compact camera orientation selector.
- [ ] Add optional beginner, teacher, and debug display modes.
- [ ] Add local example gallery entries for common scripts.
- [ ] Add import/export for desktop scripts.
- [ ] Add clearer validation summaries for invalid commands.
- [ ] Add keyboard shortcuts for playback and camera controls.

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
- [ ] Add architecture tests to guard project dependency rules.
