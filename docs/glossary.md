# Robotics Glossary

This glossary defines the introductory robotics, programming, and simulation terminology used by RobotStudio. The desktop glossary contains the same concepts with search and topic filters.

## Fundamentals

- **Axis:** A controlled direction or degree of motion. Cartesian axes translate; articulated axes usually rotate at joints.
- **Cartesian coordinate system:** A position system defined by perpendicular X, Y, and Z directions. RobotStudio expresses these coordinates in millimeters.
- **Cartesian robot:** A robot whose tool is positioned by independent linear axes, commonly X, Y, and Z.
- **Degree of freedom (DOF):** One independent way a mechanism can move.
- **Differential drive:** A mobile arrangement whose independently driven left and right wheels control translation and turning.
- **End effector:** The component attached to the robot that interacts with the environment, such as a gripper, pen, torch, or sensor.
- **Gantry robot:** A Cartesian mechanism supported by an overhead frame for movement across a rectangular workspace.
- **Homing:** Moving a robot to a known reference configuration before other positions are interpreted.
- **Joint:** A connection between links that permits rotational or linear motion.
- **Link:** A rigid robot component connected to other links through joints.
- **Origin:** The reference location where coordinate values are zero.
- **Position:** A location expressed with coordinates, without orientation.
- **Pose:** The combination of position and orientation.
- **Robot profile:** Configuration describing dimensions, movement limits, velocity limits, acceleration limits, and family-specific constraints.
- **SCARA:** Selective Compliance Assembly Robot Arm, commonly using two rotating joints for fast planar positioning.
- **Tool center point (TCP):** The reference point on an end effector whose position or pose is controlled.
- **Wheelbase:** The distance between the left and right wheel contact paths of a differential-drive robot.
- **Workspace:** The set of positions or poses a robot can physically reach while respecting geometry and limits.

## Motion

- **Acceleration:** The rate at which velocity changes over time.
- **Deceleration:** A reduction in velocity, represented as negative acceleration during a movement.
- **Effective velocity:** The speed available after applying requested speed, component limits, acceleration, and movement distance.
- **Interpolation:** Calculation of intermediate values between a movement's start and end.
- **Motion profile:** A description of how velocity changes during movement. RobotStudio uses triangular and trapezoidal profiles.
- **Path:** The geometric route followed through space, independent of timing.
- **Requested velocity:** The desired speed supplied by a command before planner limits are applied.
- **Trajectory:** A path combined with timing information.
- **Velocity:** The rate and direction of position change.

## Kinematics

- **Forward kinematics:** Calculating a tool pose from known joint or actuator positions.
- **Inverse kinematics:** Calculating joint or actuator values needed to reach a desired tool pose.
- **Kinematics:** The study of robot motion and geometry without calculating the forces causing movement.

## Simulation

- **Deterministic simulation:** A simulation that produces the same result from the same inputs and sampling interval.
- **Digital twin:** A digital representation connected to a specific physical system and its data. An educational simulation is not automatically a digital twin.
- **Odometry:** An estimate of mobile movement derived from wheel rotation or travel.
- **Playback:** Visual replay of simulation frames; it does not execute commands on hardware.
- **Robot state:** Current execution condition, such as `Idle`, `Moving`, `Homing`, `Waiting`, `Completed`, or `Faulted`.
- **Simulation:** A software model reproducing selected behavior according to explicit assumptions.
- **Simulation timestep:** The simulated time interval between calculations or samples, independent of rendering frame rate.
- **Snapshot:** A versioned representation of playback metadata, frames, poses, scene data, and movement summaries.
- **Timeline:** An ordered view of frames, commands, and state changes across simulated time.

## Programming

- **Command sequence:** An ordered collection of commands executed by the simulator.
- **Domain-specific language (DSL):** A language designed for one problem area. RobotStudio's Simple DSL represents robot actions directly.
- **Feed rate:** Movement rate written with `F` in G-code. RobotStudio interprets it in millimeters per minute.
- **G-code:** A motion-oriented command language used by CNC machines and related systems. RobotStudio currently implements an introductory Cartesian subset.
- **Parser:** Software that reads text, checks syntax, and converts valid statements into structured commands.

## Safety

- **Collision bounds:** Simplified volumes used to detect intersections between robot components, paths, and obstacles.
- **Obstacle:** An object or restricted volume that a robot must not intersect.
- **Safety limits:** Boundaries restricting position, velocity, acceleration, joint travel, or other behavior.
- **Velocity limit:** Maximum permitted speed for an axis, joint, actuator, or coordinated movement.

The glossary explains RobotStudio's current educational models. Terminology used in a specific industrial controller or robotics standard may have stricter definitions.
