# AGENTS.md

## Project Overview

**RobotStudio** is a C#/.NET project for controlling, simulating, and teaching the fundamentals of Cartesian robots.

The long-term goal is to build a desktop tool capable of:

* modeling a Cartesian robot with X/Y/Z axes;
* validating physical constraints;
* planning movements;
* simulating trajectories;
* parsing a simple scripting language or G-code subset;
* communicating with microcontrollers such as Arduino or ESP32;
* serving as educational material for programming, software architecture, and robotics classes.

At the current stage, the project must focus only on the **core domain**, **motion planning**, **tests**, and **CLI execution**. Do not implement UI, serial communication, Arduino integration, ESP32 integration, or a full DSL yet.

---

## Expected Architecture

The solution should be organized as follows:

```txt
src/
  RobotStudio.Domain/
  RobotStudio.Motion/
  RobotStudio.Simulation/
  RobotStudio.Hardware/
  RobotStudio.Scripting/
  RobotStudio.Cli/

tests/
  RobotStudio.Domain.Tests/
  RobotStudio.Motion.Tests/
```

### `RobotStudio.Domain`

Pure domain layer.

This project should contain core concepts such as:

* `Axis`
* `CartesianPosition`
* `RobotProfile`
* domain commands such as `MoveToCommand`, `HomeCommand`, and `WaitCommand`
* validation rules for physical limits

Rules:

* no UI logic;
* no console logic;
* no hardware logic;
* no file system access;
* no database access;
* no external infrastructure dependency.

The domain should be as pure and deterministic as possible.

### `RobotStudio.Motion`

Motion planning layer.

This project should contain logic for:

* linear movement planning;
* movement duration estimation;
* respecting velocity and acceleration constraints;
* generating movement plans and segments.

Suggested types:

* `MotionPlanner`
* `MotionPlan`
* `MotionSegment`

This project may depend on `RobotStudio.Domain`.

### `RobotStudio.Simulation`

Simulation layer.

Reserved for deterministic simulation of robot movements.

At this stage, keep this project lightweight. Do not over-implement simulation before the domain and motion planner are stable.

This project may depend on:

* `RobotStudio.Domain`
* `RobotStudio.Motion`

### `RobotStudio.Hardware`

Hardware integration layer.

Reserved for future serial communication, Arduino drivers, ESP32 drivers, and hardware protocols.

Do not implement real hardware communication yet unless explicitly requested.

This project may depend on `RobotStudio.Domain`, but the domain must never depend on hardware.

### `RobotStudio.Scripting`

Scripting/DSL layer.

Reserved for a future simple movement language or G-code subset.

Examples of future commands:

```txt
MOVE X=10 Y=20 Z=5 F=100
HOME
WAIT 500
```

Do not implement a full parser yet unless explicitly requested.

This project may depend on:

* `RobotStudio.Domain`
* `RobotStudio.Motion`

### `RobotStudio.Cli`

Command-line interface for testing and demonstrating the core functionality before a desktop UI exists.

The CLI may:

* create a sample robot profile;
* create initial and target positions;
* generate a motion plan;
* print the result to the console.

The CLI must not contain core business logic. It should only orchestrate existing domain and motion services.

---

## Implementation Principles

Follow these principles throughout the project:

1. Prefer clean, readable, idiomatic C#.
2. Prefer immutable types when reasonable.
3. Keep the domain model small but expressive.
4. Keep infrastructure out of the domain.
5. Avoid premature abstractions.
6. Avoid unnecessary external packages.
7. Write tests for domain and motion behavior.
8. Keep code suitable for educational use.
9. Favor explicit names over clever names.
10. Make invalid states difficult or impossible to represent when practical.

---

## C# Style Guidelines

Use modern C# features where they improve clarity:

* file-scoped namespaces;
* records for immutable domain data;
* `readonly record struct` for small value objects where appropriate;
* pattern matching where useful;
* expression-bodied members only when readability is preserved;
* nullable reference types if already enabled in the project;
* primary constructors when they improve readability.

Avoid:

* excessive inheritance;
* service locator patterns;
* static mutable state;
* large god classes;
* unnecessary dependency injection in the core domain;
* premature use of reflection or source generators.

---

## Domain Modeling Guidelines

The robot should initially be modeled as a Cartesian robot with three axes:

* X
* Y
* Z

Each axis should have:

* name;
* minimum position in millimeters;
* maximum position in millimeters;
* maximum velocity in millimeters per second;
* maximum acceleration in millimeters per second squared.

A position should represent coordinates in millimeters:

```csharp
public readonly record struct CartesianPosition(
    double X,
    double Y,
    double Z);
```

A robot profile should validate whether a position is physically reachable.

Invalid positions should fail explicitly and predictably.

---

## Motion Planning Guidelines

The first motion planner should be intentionally simple.

Implement a linear planner that:

* receives a start position;
* receives a target position;
* receives a robot profile;
* validates the target position;
* calculates displacement;
* estimates duration;
* produces a motion plan.

The planner does not need to implement advanced acceleration curves yet.

Do not implement S-curve, jerk-limited motion, PID, inverse kinematics, or collision detection at this stage.

For the first version:

* zero-distance movement should be handled predictably;
* valid movement should produce a valid plan;
* invalid movement should throw or return a clear domain-level error;
* estimated duration should be greater than zero when displacement exists.

---

## Testing Guidelines

Use xUnit for automated tests.

At minimum, tests should cover:

* valid positions;
* invalid positions;
* boundary positions;
* motion planning for valid movement;
* rejection of invalid target positions;
* zero-distance movement;
* positive duration for non-zero movement.

Tests should be simple, deterministic, and readable.

Prefer test names in this style:

```csharp
MethodName_WhenCondition_ShouldExpectedBehavior()
```

Example:

```csharp
ValidatePosition_WhenXIsOutsideLimits_ShouldThrow()
```

---

## CLI Guidelines

The CLI should demonstrate the current capabilities of the system.

A basic CLI run should:

1. create a robot profile;
2. create a start position;
3. create a target position;
4. generate a motion plan;
5. print the movement summary.

Example output style:

```txt
RobotStudio CLI

Start position:
X=0mm Y=0mm Z=0mm

Target position:
X=120mm Y=80mm Z=20mm

Motion plan:
Distance: 145.60mm
Estimated duration: 1.46s
Segments: 1
```

The CLI should remain simple. Do not build an interactive shell yet unless explicitly requested.

---

## Commands

Use these commands to validate the project:

```bash
dotnet build
dotnet test
dotnet run --project src/RobotStudio.Cli
```

If a command fails because of SDK version or local environment configuration, explain the reason clearly and avoid unrelated code changes.

---

## Documentation Guidelines

Keep documentation short, practical, and useful.

The root `README.md` should explain:

* what RobotStudio is;
* current project status;
* solution structure;
* how to build;
* how to test;
* how to run the CLI.

The root `TODO.md` should track milestones such as:

```txt
Milestone 1 — Mathematical Core
Milestone 2 — Motion Planning
Milestone 3 — Scripting/DSL
Milestone 4 — Simulation
Milestone 5 — Hardware Communication
Milestone 6 — Desktop UI
```

Do not mark incomplete work as done.

---

## Current Priorities

The current priority is to produce a functional, testable, educational foundation.

Focus on:

1. domain model;
2. physical validation;
3. motion planner;
4. automated tests;
5. CLI example;
6. concise documentation.

Do not focus on:

* UI;
* Avalonia;
* WPF;
* MAUI;
* serial communication;
* Arduino;
* ESP32;
* advanced simulation;
* full scripting language;
* database;
* web API;
* authentication;
* cloud deployment.

---

## Design Constraints

RobotStudio should be designed as a real system, not as a toy example.

However, the code should also be teachable.

When choosing between a highly abstract solution and a clear solution, prefer the clear solution.

When choosing between a quick hack and a clean small abstraction, prefer the clean small abstraction.

When the future architecture is obvious but not yet needed, leave the code prepared without implementing unnecessary layers.

---

## Pull Request / Commit Expectations

Before considering a task complete:

```bash
dotnet build
dotnet test
```

should pass.

When relevant, also run:

```bash
dotnet run --project src/RobotStudio.Cli
```

Commits should be focused and descriptive.

Good examples:

```txt
Add Cartesian robot domain model
Add linear motion planner
Add motion planner tests
Update CLI movement example
Document initial project structure
```

Avoid vague messages such as:

```txt
Update files
Fix stuff
Changes
WIP
```

---

## Future Direction

After the core is stable, the project may evolve toward:

* deterministic simulation;
* visual timeline;
* 2D/3D robot preview;
* simple movement scripting language;
* G-code subset;
* serial protocol;
* Arduino/ESP32 firmware examples;
* Avalonia desktop UI;
* course-ready lessons and exercises.

Do not implement these future features prematurely.

The immediate goal is a strong foundation.
