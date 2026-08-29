# TODO

This file tracks future work after the `1.2.0` didactic tooling and mechanical visualization release. Completed release history belongs in `CHANGELOG.md`.

## 1. Release And Distribution

- [ ] Create and push the `v1.2.0` Git tag after the release commit passes CI.
- [ ] Acquire a code signing certificate before publishing signed Windows releases.
- [ ] Add Windows ARM64 CLI release artifacts if student machines need them.

## 2. Future Robot Family Expansion

- [ ] Implement the Cylindrical Robot as the first mixed revolute/prismatic teaching model.
- [ ] Implement the Ackermann Steering Robot for car-like steering geometry and non-holonomic motion.
- [ ] Implement the Omnidirectional Robot for holonomic movement and wheel-speed decomposition.
- [ ] Implement the Self-Balancing Robot after dynamic simulation, sensors, and feedback-control infrastructure exist.
- [ ] Implement the Stewart Platform as the advanced six-actuator parallel mechanism.
- [ ] Implement the Mobile Manipulator as a capstone that coordinates a mobile base and articulated arm.
- [ ] Expand the catalog by mapping new robots for implementation.
- [ ] Add tests before each new robot family is considered complete.

## 3. Hardware Integration

- [ ] Define the first serial protocol draft.
- [ ] Choose the first supported educational controller board.
- [ ] Choose the first supported motor driver setup.
- [ ] Implement serial port discovery.
- [ ] Implement connection open, close, and health checks.
- [ ] Implement command transmission in dry-run mode before enabling real motion.
- [ ] Add hardware safety limits before any real execution path.
- [ ] Add Arduino or ESP32 firmware examples only after the protocol is stable.

## 4. Testing And Quality

- [x] Split the oversized WPF `MainWindow` code-behind into cohesive partial files and extract shared non-behavioral window resources.
- [ ] Extract robot workspaces from `MainWindow.xaml` into dedicated controls and presenters when the next desktop architecture pass begins.
- [ ] Add code coverage reporting when coverage goals are defined.
- [ ] Add stricter analyzers when the coding standard becomes more mature.
- [ ] Add package vulnerability scanning when external dependencies become more relevant.
- [ ] Add UI smoke tests if the desktop workflow becomes stable enough to automate.
