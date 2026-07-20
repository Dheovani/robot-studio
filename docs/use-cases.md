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

## Planned Use Cases

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

### Observe A Robot In A Future 3D View

Actor: student.

Goal: inspect robot movement from multiple camera angles.

Current status: future work.

Expected result:

- visualization consumes simulation output;
- students can observe mechanical behavior;
- visual controls do not change domain rules.

### Send Commands To Real Hardware

Actor: instructor or advanced student.

Goal: send validated robot commands to Arduino or ESP32 hardware.

Current status: future work.

Expected result:

- hardware adapters consume validated commands;
- simulator remains usable without physical devices;
- hardware failures do not affect domain purity.
