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
- the SCARA robot appears as `Planned`;
- the simple articulated arm appears as `Planned`;
- the delta robot appears as `Planned`;
- the drone appears as `Planned`;
- the 6-DOF industrial arm appears as `Planned`;
- each robot card shows family, status, complexity, description, and capabilities;
- planned robot entries cannot be opened yet;
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

### Execute A Command Sequence

Actor: student.

Goal: execute commands such as `HOME`, `MOVE`, `WAIT`, and another `MOVE` in order.

Current status: implemented for a simple deterministic simulation flow.

Expected result:

- commands execute deterministically;
- the simulator records position and state changes;
- invalid commands produce a `Faulted` simulation result.

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
- students can use the mouse wheel or zoom slider to inspect the robot from closer or farther away;
- students can reset the camera or choose predefined views;
- workspace limits are visible without covering the robot mechanism;
- students can inspect current state, position, command, source line, and simulated time;
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
