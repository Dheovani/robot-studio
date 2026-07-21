# Next Phase Plan

RobotStudio's next phase turns the current deterministic robotics core into a visual, didactic desktop experience. The project remains a robotics/programming teaching platform, not a lesson manager or LMS.

## Product Direction

The desktop app should let students choose a robot family, inspect a 3D mechanism, execute commands through multiple input styles, and understand why the robot moved the way it did.

The Cartesian robot remains the first fully functional robot. Future robot families may appear as planned placeholders before they receive full simulation support.

## Milestones

### 1. Cartesian Viewer Usability

- [x] Add orbit camera controls.
- [x] Add zoom control.
- [x] Add reset camera command.
- [x] Add basic predefined views: front, side, top, and isometric.
- [x] Add a state panel with time, state, position, command, and source line.
- [x] Keep the viewer consuming `CartesianPlaybackSnapshot` and `SceneFrames`.

### 2. Robot Selection Shell

- [ ] Add descriptors for robot families and templates.
- [ ] Add the first robot selection screen.
- [ ] Show Cartesian robot as available.
- [ ] Show articulated arm and drone as planned.
- [ ] List capabilities without implementing unavailable robots.

### 3. Desktop Script Workflow

- [ ] Add a DSL script editor panel.
- [ ] Add validate and simulate buttons.
- [ ] Show parser errors with line numbers.
- [ ] Highlight the script line that produced the current playback frame.
- [ ] Keep G-code out of this milestone.

### 4. Manual Cartesian Control

- [ ] Add `HOME` button.
- [ ] Add jog buttons for X, Y, and Z.
- [ ] Add step size in millimeters.
- [ ] Add requested velocity.
- [ ] Generate simulation commands from manual actions.
- [ ] Optionally generate a script from manual actions.

### 5. Didactic Overlays

- [ ] Toggle workspace visibility.
- [ ] Toggle global axes.
- [ ] Toggle grid.
- [ ] Toggle TCP marker.
- [ ] Toggle planned path.
- [ ] Toggle start/end markers.

### 6. Timeline And Explanations

- [ ] Add frame-by-frame stepping.
- [ ] Add playback speed control.
- [ ] Add command markers.
- [ ] Add state markers.
- [ ] Add movement explanation text.
- [ ] Explain effective velocity versus requested velocity when applicable.

### 7. Charts

- [ ] Plot X/Y/Z position over time.
- [ ] Plot state over time.
- [ ] Plot requested versus effective velocity.
- [ ] Plot total distance.

### 8. Future Interfaces

- [ ] Prepare G-code as a second parser dialect that produces domain commands.
- [ ] Prepare hardware command boundaries without serial implementation.
- [ ] Keep Arduino, ESP32, and real hardware communication out until the simulator and desktop flows are stable.

## Proposed Robot Metadata

The desktop shell should list robots from small metadata descriptors instead of hard-coding every card in the window.

Proposed types:

- `RobotFamilyDescriptor`: identifies a robot family such as Cartesian, articulated arm, or drone.
- `RobotTemplate`: describes a selectable robot template.
- `RobotCapability`: lists supported capabilities such as simulation, scripting, 3D view, manual control, hardware, or G-code.
- `RobotViewerDescriptor`: describes which desktop viewer can open a robot template.

Initial capabilities:

- `Simulation`
- `ScriptExecution`
- `ThreeDimensionalView`
- `ManualControl`
- `HardwareCommunication`
- `GCode`

## Architectural Rules

- Domain stays free of UI, rendering, files, and hardware.
- Motion stays free of UI.
- Simulation produces contracts consumed by UI.
- Desktop consumes snapshots, scene frames, poses, and viewport data.
- UI may own camera interaction and view state.
- UI must not duplicate simulation or domain validation rules.
- Hardware and G-code remain planned, not implemented, until explicitly started.
