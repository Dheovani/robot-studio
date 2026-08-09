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

### Edit And Simulate A DSL Script In The Desktop App

Actor: student.

Goal: change a Cartesian robot script and immediately inspect the simulated result.

Current status: implemented for the first Cartesian WPF viewer.

Expected result:

- the student edits DSL text in the Cartesian viewer;
- the script editor shows line numbers and simple command tags for `HOME`, `MOVE`, and `WAIT`;
- `Validate` reports parser or physical limit errors without running playback;
- `Simulate` regenerates playback from the current script when the script is valid;
- playback displays the command source line associated with the current frame;
- G-code remains out of scope for this workflow, but the scripting boundary can accept future dialects that produce the same command sequence.

### Jog A Cartesian Robot Manually In The Desktop App

Actor: student.

Goal: move the Cartesian robot without typing the full command manually.

Current status: implemented for the first Cartesian WPF viewer.

Expected result:

- the student chooses a step size in millimeters;
- the student chooses a requested speed in millimeters per second;
- `HOME` appends a `HOME` command to the DSL script;
- jog buttons append `MOVE` commands to the DSL script;
- each manual action reuses the DSL parser and simulator;
- invalid manual movements report the same domain validation errors as scripts.

### Execute A Direct Command In The Desktop App

Actor: student.

Goal: type and execute one DSL command without editing the full script manually.

Current status: implemented for the first Cartesian WPF viewer.

Expected result:

- the student types one DSL command in the command console;
- pressing `Enter` or `Execute` runs the command;
- accepted commands are appended to the DSL script;
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
