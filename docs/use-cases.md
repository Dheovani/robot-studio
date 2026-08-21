# Use Cases

## Current Use Cases

### Generate A Cartesian Motion Plan From Code

Actor: developer or instructor.

Goal: create a robot profile, define a start position and target position, and generate a simple linear motion plan.

Current status: implemented through domain and motion planner types.

Expected result:

- valid positions produce a motion plan;
- invalid positions fail with a clear domain error;
- zero-distance movement produces a predictable stationary plan.

### Run The CLI Example

Actor: student, developer, or instructor.

Goal: run the current CLI and inspect a readable motion plan example.

Current status: implemented as a hard-coded CLI example.

Command:

```bash
dotnet run --project src/RobotStudio.Cli
```

### Select A Robot In The Desktop App

Actor: student.

Goal: choose a robot model before opening a simulator.

Current status: implemented for the first WPF desktop shell.

Expected result:

- the desktop app lists available and planned robot templates in didactic complexity order;
- the Cartesian robot appears as `Available`;
- the XY plotter appears as `Available`;
- the differential drive robot appears as `Available`;
- the SCARA robot appears as `Available`;
- the simple articulated arm appears as `Available`;
- the delta robot appears as `Available`;
- the drone appears as `Available`;
- the 6-DOF industrial arm appears as `Available`;
- the cylindrical, Ackermann steering, omnidirectional, self-balancing, Stewart platform, and mobile manipulator models appear as `Planned`;
- each robot card shows family, status, complexity, description, and capabilities;
- future planned robot entries cannot be opened until they have a concrete viewer;
- the selection screen remains a simulator entry point, not a lesson manager.

### Open The XY Plotter

Actor: student.

Goal: inspect the beginner two-axis Cartesian-family model before advancing to mobile or articulated robots.

Current status: implemented as a fixed-plane visual simulation backed by an XY plotter domain profile and motion planner.

Expected result:

- the XY plotter opens from the robot selection screen;
- the example script moves only through X/Y commands with `Z=0`;
- attempts to move the plotter away from the drawing plane are rejected;
- Z jog buttons are disabled in the desktop viewer.

### Plan Differential Drive Movement From Code

Actor: developer or instructor.

Goal: introduce mobile robot motion where a pose includes planar position and heading.

Current status: implemented for domain, motion planning, deterministic simulation, playback sampling, and the first 2D desktop viewer.

Expected result:

- a differential drive pose stores `X`, `Y`, and heading in degrees;
- the profile validates planar workspace limits and physical robot dimensions;
- the planner separates translation and rotation segments;
- requested linear and angular velocities are capped by profile limits;
- the simulator executes `HOME`, differential-drive move commands, and `WAIT`;
- the desktop viewer shows the workspace, path, robot pose, heading, and frame timeline.
- the student can edit and simulate mobile DSL commands with `DRIVE X=... Y=... HEADING=... LIN=... ANG=...`.

### Simulate SCARA Joint Movement From Code

Actor: developer or instructor.

Goal: introduce an articulated planar robot where movement is expressed through joints and the tool pose is calculated by kinematics.

Current status: implemented for domain, forward/inverse kinematics, motion planning, deterministic simulation, playback sampling, simple DSL, and the first 3D desktop viewer.

Expected result:

- a SCARA profile defines link lengths and joint limits;
- joint positions validate shoulder and elbow angles;
- forward kinematics converts joint angles into tool X/Y pose;
- inverse kinematics calculates an elbow-down joint solution for reachable tool poses;
- the simulator executes `HOME`, SCARA joint moves, and `WAIT`;
- the desktop viewer shows reachable workspace, volumetric links, current tool pose, planned path, state, command name, camera controls, and frame timeline;
- the student can edit and simulate SCARA DSL commands with `SCARA SHOULDER=... ELBOW=... SPEED=...`.

### Simulate Simple Articulated Arm Movement From Code

Actor: developer or instructor.

Goal: introduce a three-joint articulated arm where base, shoulder, and elbow angles compose into a tool pose through forward kinematics.

Current status: implemented for domain, forward kinematics, motion planning, deterministic simulation, playback sampling, simple DSL, and the first 3D desktop viewer.

Expected result:

- a simple arm profile defines three link lengths and three joint limits;
- joint positions validate base, shoulder, and elbow angles;
- forward kinematics converts joint angles into X/Y tool pose and orientation;
- the planner coordinates joint-space movement and caps velocity by involved joint limits;
- the simulator executes `HOME`, simple arm joint moves, and `WAIT`;
- the desktop viewer shows reachable workspace, volumetric base, three links, current tool pose, tool orientation, planned path, state, command name, camera controls, and frame timeline;
- the student can edit and simulate simple arm DSL commands with `ARM BASE=... SHOULDER=... ELBOW=... SPEED=...`.

### Simulate 6-DOF Industrial Arm Movement

Actor: student or instructor.

Goal: study how six revolute joints coordinate the position and orientation of an industrial-arm TCP.

Current status: implemented for domain limits, simplified forward kinematics, coordinated joint planning, deterministic simulation, playback sampling, simple DSL, and a 3D desktop viewer.

Expected result:

- an industrial-arm profile defines four physical dimensions and individual limits for `J1` through `J6`;
- the planner coordinates involved joints and caps movement by the slowest joint limit;
- the simulator executes `HOME`, `ARM6`, and `WAIT`;
- the viewer shows the serial links, six joint markers, TCP orientation, reachable area, planned path, state, command, camera orbit, zoom, and timeline;
- the student can load examples or edit `ARM6 J1=... J2=... J3=... J4=... J5=... J6=... SPEED=...` commands.

### Execute A Command Sequence

Actor: student.

Goal: execute commands such as `HOME`, `MOVE`, `WAIT`, and another `MOVE` in order.

Current status: implemented for a simple deterministic simulation flow.

Expected result:

- commands execute deterministically;
- the simulator records position and state changes;
- invalid commands produce a `Faulted` simulation result.

### Recover A Faulted Simulation

Actor: application service, CLI, or future desktop session controller.

Goal: acknowledge a simulation fault without pretending that the robot physically returned home.

Current status: implemented in the domain, Simple DSL, and every available family simulator; desktop session controls remain future work.

Expected result:

- the failed execution retains its timeline and exposes the last valid state through `FinalContext`;
- executing `RESET` from that context changes `Faulted` to `Idle`;
- robot pose, joints, actuators, odometry, and elapsed simulated time remain unchanged;
- executing `HOME` from the faulted context instead performs the family's planned homing movement;
- `RESET` from a non-faulted state is rejected with an explicit state-transition error.
- the desktop `Reset Fault` action is enabled only for a retained `Faulted` context;
- the desktop `HOME` action executes homing from the retained family-specific context instead of rebuilding a clean simulation;
- validation does not mutate or replace the retained desktop session.

### Reject An Obstructed Cartesian Path

Actor: student or simulation host.

Goal: demonstrate that a target can be inside the robot limits while the straight path to it is unsafe.

Current status: implemented in the Cartesian simulation core; obstacle editing and rendering remain future desktop work.

Expected result:

- the host defines one or more immutable axis-aligned obstacle volumes in a `CartesianSimulationEnvironment`;
- `MOVE` and `HOME` test their complete linear paths before movement begins;
- touching or crossing an obstacle produces a `Faulted` result;
- the failure identifies the obstacle, first collision point, and fraction of the requested trajectory;
- the robot remains at its last valid position and no simulated movement time is added;
- paths that avoid every obstacle execute normally.

### Reject An Obstructed Differential Drive Path

Actor: student or simulation host.

Goal: demonstrate why a mobile robot's center path can be clear while its physical body still hits an obstacle.

Current status: implemented in the Differential Drive simulation core; obstacle editing and rendering remain future desktop work.

Expected result:

- the robot profile defines an explicit circular collision radius;
- the host defines rectangular obstacles in a `PlanarSimulationEnvironment`;
- `DRIVE` and `HOME` test the swept circular footprint, including exact side and rounded-corner contact;
- a collision reports the obstacle, robot-center pose, physical contact point, and trajectory fraction;
- blocked commands preserve pose, ideal wheel odometry, and elapsed simulated time;
- clear paths continue through normal translation and rotation playback.

### Reject An Obstructed SCARA Joint Path

Actor: student or simulation host.

Goal: show that articulated-robot safety depends on every moving link, not only the destination of the tool.

Current status: implemented in the SCARA simulation core using deterministic joint-space sampling; obstacle editing and rendering remain future desktop work.

Expected result:

- the SCARA profile defines a physical radius for both links;
- each sampled configuration treats the first and second links as capsules;
- `SCARA` and `HOME` inspect intermediate joint configurations against planar obstacles;
- a blocked movement identifies the obstacle, colliding link, sampled joints, and trajectory fraction;
- the command faults before execution and preserves the last valid joints and simulated time;
- changing the configurable maximum angular sample step changes resolution without changing simulation determinism.

### Reject Spatial Robot Collisions

Actor: student or simulation host.

Goal: compare how collision concepts differ across serial arms, parallel robots, and aerial robots.

Current status: baseline deterministic collision envelopes are implemented for every available family; desktop obstacle authoring and overlays remain future work.

Expected result:

- Simple and 6-DOF articulated arms inspect every derived link during sampled joint-space motion;
- Delta movement inspects the moving platform and all three carriage-to-platform links during sampled actuator motion;
- Drone movement inspects a spherical body envelope along the complete 3D translation path;
- failures identify the obstacle and semantic component that caused the obstruction;
- blocked commands fault before execution and preserve the last valid physical state and elapsed time;
- each family continues using its own kinematics rather than a Cartesian-only collision assumption.

### Simulate A Simple DSL Script

Actor: student.

Goal: run a beginner-friendly script.

Example:

```txt
HOME
MOVE X=10 Y=20 Z=5 SPEED=100
WAIT 500
MOVE X=0 Y=0 Z=0 SPEED=80
```

Current status: implemented through the CLI `simulate` command.

Expected result:

- the script is parsed into domain commands;
- commands are executed by the simulator;
- the CLI prints final state, final position, and total simulated duration.

### Validate A Script Before Execution

Actor: student.

Goal: check whether a script is valid without executing it.

Current status: implemented through the CLI `validate` command.

Expected result:

- syntax errors include line numbers;
- invalid numbers are reported clearly;
- invalid positions are reported with the expected axis limits.

### Compare Simple DSL And G-Code

Actor: student.

Goal: execute the same Cartesian lesson using two command syntaxes.

Current status: implemented in the Cartesian and XY Plotter desktop workspaces.

Expected result:

- the student selects Simple DSL or G-code before validating or simulating;
- local examples are presented in the selected dialect;
- `HOME` and `G28`, `MOVE` and `G1`, and `WAIT` and `G4` produce the same domain command types;
- `G90` absolute and `G91` relative movements resolve to absolute domain targets using the viewer's initial position and prior G-code movements;
- manual jog and direct console actions append syntax matching the selected dialect;
- switching syntax does not duplicate domain, motion, or simulation rules.

### Run A Selected Script Dialect In The CLI

Actor: student.

Goal: validate, simulate, sample, or export a Cartesian script without changing execution rules between Simple DSL and G-code.

Current status: implemented.

Expected result:

- `.robot` files use Simple DSL and `.gcode` files use G-code automatically;
- `--dialect dsl|gcode` overrides extension inference for neutral files and comparisons;
- the CLI reports the selected dialect;
- validation, simulation, playback, and snapshot export consume the resolved `IRobotScriptDialect`;
- unsupported dialect names and malformed options return clear argument errors.

### Study Cartesian Validation And Sequencing Examples

Actor: student.

Goal: compare valid motion, constrained speed, intentional validation failure, and state sequencing without writing the first script from scratch.

Current status: implemented in the Cartesian desktop example selector and `examples/cartesian/`.

Expected result:

- the axis-limit lesson intentionally targets `X=320 mm` and explains the valid `0..300 mm` range when validated;
- the speed lesson compares an accepted X request with Y and Z requests capped by their axis limits;
- charts and movement explanations show requested and effective speed without reparsing source text;
- movement explanations identify triangular and trapezoidal profiles, velocity and acceleration limiting axes, exact phase durations, and the active playback phase;
- the jog-style lesson mirrors small X+, Y+, and Z+ actions as an ordered command sequence;
- waits visibly retain position while advancing simulated time;
- the final home command returns the robot to the origin;
- Simple DSL and G-code variants preserve the same lesson intent.

### Observe A Robot In A 3D View

Actor: student.

Goal: inspect robot movement from multiple camera angles.

Current status: implemented for the first Cartesian WPF viewer.

Expected result:

- visualization consumes simulation output;
- students can observe mechanical behavior;
- students can drag inside the viewport to rotate the camera around the robot;
- students can use `Ctrl+mouse wheel` or zoom controls to inspect the robot from closer or farther away;
- students can reset the camera or choose predefined views;
- workspace limits are visible without covering the robot mechanism;
- students can inspect current state, position, command, source line, and simulated time;
- students can inspect ideal left/right wheel odometry while a Differential Drive command is playing;
- students can observe Drone roll, pitch, and yaw attitude changing with the schematic 3D body;
- visual controls do not change domain rules.

### Edit And Simulate A Script In The Desktop App

Actor: student.

Goal: change a Cartesian robot script and immediately inspect the simulated result.

Current status: implemented for the first Cartesian WPF viewer.

Expected result:

- the student selects Simple DSL or G-code and edits text in the Cartesian viewer;
- the script editor shows line numbers and command tags for `HOME`/`G28`, `MOVE`/`G1`, and `WAIT`/`G4`;
- `Validate` reports parser or physical limit errors without running playback;
- `Simulate` regenerates playback from the current script when the script is valid;
- playback displays the command source line associated with the current frame;
- the selected dialect produces the shared domain command sequence consumed by the same simulator.

### Jog A Cartesian Robot Manually In The Desktop App

Actor: student.

Goal: move the Cartesian robot without typing the full command manually.

Current status: implemented for the first Cartesian WPF viewer.

Expected result:

- the student chooses a step size in millimeters;
- the student chooses a requested speed in millimeters per second;
- homing appends `HOME` or `G28` according to the selected dialect;
- jog buttons append `MOVE` or `G1` according to the selected dialect;
- each manual action reuses the selected parser and the same simulator;
- invalid manual movements report the same domain validation errors as scripts.

### Configure A Cartesian Robot Profile

Actor: student or instructor.

Goal: observe how physical axis limits, velocity, and acceleration change validation, workspace geometry, and playback.

Current status: implemented for the Cartesian desktop workspace.

Expected result:

- the user edits X/Y/Z minimum, maximum, maximum velocity, and maximum acceleration values;
- parsing uses invariant decimal notation and rejects blank, malformed, or non-finite values;
- domain axis invariants reject invalid ranges and non-positive motion limits;
- every accepted profile includes the fixed HOME position at `(0, 0, 0)`;
- applying a profile resets simulation state to HOME and regenerates the current script;
- workspace bounds, camera framing, motion planning, charts, and explanations consume the new profile;
- an incompatible script remains editable while a safe HOME preview replaces stale playback;
- restoring returns to the default Cartesian teaching profile.

### Execute A Direct Command In The Desktop App

Actor: student.

Goal: type and execute one command in the selected dialect without editing the full script manually.

Current status: implemented for the first Cartesian WPF viewer.

Expected result:

- the student types one Simple DSL or G-code command in the command console;
- pressing `Enter` or `Execute` runs the command;
- accepted commands are appended to the current script;
- rejected commands are reported without closing the app;
- command history records accepted and rejected command attempts;
- direct commands reuse the same parser and simulator as scripts and manual controls.

## Planned Future Use Cases

### Send Commands To Real Hardware

Actor: instructor or advanced student.

Goal: send validated robot commands to Arduino or ESP32 hardware.

Current status: future work.

Expected result:

- hardware adapters consume validated commands;
- simulator remains usable without physical devices;
- hardware failures do not affect domain purity.
