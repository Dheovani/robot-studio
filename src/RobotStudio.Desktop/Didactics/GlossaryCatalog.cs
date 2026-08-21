namespace RobotStudio.Desktop.Didactics;

public static class GlossaryCatalog
{
    private static readonly GlossaryEntry[] Entries =
    [
        Entry("Acceleration", GlossaryCategory.Motion,
            "The rate at which velocity changes over time. RobotStudio expresses linear acceleration in millimeters per second squared and angular acceleration in degrees per second squared.",
            ["Deceleration", "Motion profile", "Velocity"]),
        Entry("Axis", GlossaryCategory.Fundamentals,
            "A controlled direction or degree of motion. A Cartesian robot commonly uses linear X, Y, and Z axes, while articulated robots use rotating joint axes.",
            ["Degree of freedom", "Joint", "Workspace"]),
        Entry("Cartesian coordinate system", GlossaryCategory.Fundamentals,
            "A position system defined by perpendicular X, Y, and Z directions. RobotStudio uses millimeters for Cartesian coordinates.",
            ["Pose", "Position", "Workspace"]),
        Entry("Cartesian robot", GlossaryCategory.Fundamentals,
            "A robot whose tool is positioned by independent linear axes, typically X, Y, and Z. Its rectangular structure makes coordinate movement especially suitable for introductory lessons.",
            ["Axis", "Gantry", "Tool center point"]),
        Entry("Collision bounds", GlossaryCategory.Safety,
            "Simplified volumes used to detect whether robot components or paths intersect obstacles. Bounds are safety approximations rather than detailed mechanical meshes.",
            ["Obstacle", "Safety limits", "Workspace"]),
        Entry("Command sequence", GlossaryCategory.Programming,
            "An ordered collection of robot commands executed by the simulator. Command order determines the resulting states, positions, and elapsed time.",
            ["DSL", "G-code", "Robot state"]),
        Entry("Deceleration", GlossaryCategory.Motion,
            "A reduction in velocity. In RobotStudio it appears as negative acceleration while a moving robot approaches the end of a planned segment.",
            ["Acceleration", "Motion profile", "Velocity"]),
        Entry("Degree of freedom", "DOF", GlossaryCategory.Fundamentals,
            "One independent way a mechanism can move. A three-axis Cartesian robot has three translational degrees of freedom, while an industrial arm may have six rotational degrees of freedom.",
            ["Axis", "Joint", "Pose"]),
        Entry("Deterministic simulation", GlossaryCategory.Simulation,
            "A simulation that produces the same result from the same profile, initial state, commands, and sampling interval. It avoids hidden randomness so behavior can be tested and taught reliably.",
            ["Playback", "Simulation timestep", "Snapshot"]),
        Entry("Differential drive", GlossaryCategory.Fundamentals,
            "A mobile robot arrangement with independently driven left and right wheels. Differences in wheel travel make the robot move straight, follow an arc, or rotate.",
            ["Odometry", "Pose", "Wheelbase"]),
        Entry("Digital twin", GlossaryCategory.Simulation,
            "A digital representation connected to a specific physical system and its data. RobotStudio simulations are educational models and should not be described as digital twins without that real-system connection.",
            ["Simulation", "Robot profile"]),
        Entry("Domain-specific language", "DSL", GlossaryCategory.Programming,
            "A small language designed for one problem area. RobotStudio's Simple DSL expresses robot actions such as HOME, MOVE, WAIT, DRIVE, and joint commands.",
            ["Command sequence", "G-code", "Parser"]),
        Entry("Effective velocity", GlossaryCategory.Motion,
            "The velocity the planner can actually use after applying requested speed, axis or joint limits, acceleration, and movement distance.",
            ["Requested velocity", "Motion profile", "Velocity limit"]),
        Entry("End effector", GlossaryCategory.Fundamentals,
            "The component attached to the end of a robot that interacts with the environment, such as a gripper, pen, welding torch, or sensor.",
            ["Tool center point", "Pose", "Trajectory"]),
        Entry("Feed rate", GlossaryCategory.Programming,
            "The requested movement rate written with F in the introductory G-code dialect. RobotStudio interprets it in millimeters per minute and converts it to millimeters per second.",
            ["G-code", "Requested velocity"]),
        Entry("Forward kinematics", GlossaryCategory.Kinematics,
            "The calculation of a tool pose from known joint or actuator positions. It answers where the end effector is for a given robot configuration.",
            ["Inverse kinematics", "Joint", "Pose"]),
        Entry("G-code", GlossaryCategory.Programming,
            "A command language widely used by CNC machines and related motion systems. RobotStudio currently supports a small Cartesian teaching subset rather than every G-code command.",
            ["DSL", "Feed rate", "Parser"]),
        Entry("Gantry robot", GlossaryCategory.Fundamentals,
            "A Cartesian mechanism supported by an overhead frame, often allowing a tool or load to move across a large rectangular workspace.",
            ["Cartesian robot", "Workspace"]),
        Entry("Homing", GlossaryCategory.Fundamentals,
            "The process of moving a robot to a known reference configuration. RobotStudio currently fixes HOME at the defined origin or home joint configuration for each robot family.",
            ["Origin", "Robot state", "Safety limits"]),
        Entry("Interpolation", GlossaryCategory.Motion,
            "The calculation of intermediate values between a movement's start and end. Playback uses interpolation to show positions between command boundaries.",
            ["Motion profile", "Playback", "Trajectory"]),
        Entry("Inverse kinematics", GlossaryCategory.Kinematics,
            "The calculation of joint or actuator values needed to reach a desired tool pose. A target may have multiple solutions, one solution, or no reachable solution.",
            ["Forward kinematics", "Joint", "Workspace"]),
        Entry("Joint", GlossaryCategory.Fundamentals,
            "A connection between robot links that permits motion. Revolute joints rotate and prismatic joints translate.",
            ["Axis", "Degree of freedom", "Link"]),
        Entry("Kinematics", GlossaryCategory.Kinematics,
            "The study of robot motion and geometry without calculating the forces that cause the movement.",
            ["Forward kinematics", "Inverse kinematics", "Pose"]),
        Entry("Link", GlossaryCategory.Fundamentals,
            "A rigid robot component connected to other links through joints. Link lengths and joint angles determine an articulated robot's geometry.",
            ["Forward kinematics", "Joint"]),
        Entry("Motion profile", GlossaryCategory.Motion,
            "A description of how velocity changes during a movement. RobotStudio uses acceleration-aware triangular or trapezoidal profiles.",
            ["Acceleration", "Effective velocity", "Interpolation"]),
        Entry("Odometry", GlossaryCategory.Simulation,
            "An estimate of mobile robot movement derived from wheel rotation or travel. RobotStudio's current differential-drive odometry is ideal and does not model slip or encoder noise.",
            ["Differential drive", "Pose", "Wheelbase"]),
        Entry("Obstacle", GlossaryCategory.Safety,
            "An object or restricted volume that a robot path or component must not intersect. RobotStudio uses deterministic simplified obstacles for introductory collision checks.",
            ["Collision bounds", "Safety limits", "Workspace"]),
        Entry("Origin", GlossaryCategory.Fundamentals,
            "The reference location where coordinate values are zero. The introductory Cartesian HOME position is fixed at X=0, Y=0, Z=0.",
            ["Cartesian coordinate system", "Homing", "Position"]),
        Entry("Parser", GlossaryCategory.Programming,
            "Software that reads script text, checks its syntax, and converts valid statements into structured commands understood by the application.",
            ["Command sequence", "DSL", "G-code"]),
        Entry("Path", GlossaryCategory.Motion,
            "The geometric route followed through space, independent of how quickly it is traversed.",
            ["Trajectory", "Tool center point", "Workspace"]),
        Entry("Playback", GlossaryCategory.Simulation,
            "The visual replay of deterministic simulation frames. Playback controls visualization and does not send commands to physical hardware.",
            ["Simulation timestep", "Snapshot", "Timeline"]),
        Entry("Position", GlossaryCategory.Fundamentals,
            "A location expressed with coordinates. Unlike a pose, position does not include orientation.",
            ["Cartesian coordinate system", "Pose"]),
        Entry("Pose", GlossaryCategory.Fundamentals,
            "The combination of an object's position and orientation. Some introductory robots need only position, while drones and articulated tools also require orientation.",
            ["Cartesian coordinate system", "Position", "Tool center point"]),
        Entry("Requested velocity", GlossaryCategory.Motion,
            "The desired speed supplied by a command. The planner may reduce it to respect the robot profile and the distance available for acceleration.",
            ["Effective velocity", "Feed rate", "Velocity limit"]),
        Entry("Robot profile", GlossaryCategory.Fundamentals,
            "The configuration that defines a robot's dimensions, movement limits, velocity limits, acceleration limits, and other family-specific constraints.",
            ["Safety limits", "Workspace"]),
        Entry("Robot state", GlossaryCategory.Simulation,
            "The simulator's current execution condition, such as Idle, Moving, Homing, Waiting, Completed, or Faulted.",
            ["Command sequence", "Deterministic simulation", "Homing"]),
        Entry("Safety limits", GlossaryCategory.Safety,
            "Configured boundaries that restrict position, velocity, acceleration, joint travel, or other robot behavior before execution is accepted.",
            ["Collision bounds", "Robot profile", "Velocity limit", "Workspace"]),
        Entry("SCARA", GlossaryCategory.Fundamentals,
            "A Selective Compliance Assembly Robot Arm, commonly using two rotating joints for fast planar positioning with controlled vertical tooling.",
            ["Forward kinematics", "Joint", "Workspace"]),
        Entry("Simulation", GlossaryCategory.Simulation,
            "A software model that reproduces selected robot behavior. Its accuracy depends on the assumptions included, so a simulation is not automatically a complete model of physical reality.",
            ["Deterministic simulation", "Digital twin", "Playback"]),
        Entry("Simulation timestep", GlossaryCategory.Simulation,
            "The simulated time interval between calculations or samples. It is independent from the rendering frame rate and should remain deterministic.",
            ["Deterministic simulation", "Playback", "Snapshot"]),
        Entry("Snapshot", GlossaryCategory.Simulation,
            "A versioned data representation of playback metadata, frames, poses, scene information, and movement summaries that can be exported and validated.",
            ["Playback", "Simulation timestep", "Timeline"]),
        Entry("Tool center point", "TCP", GlossaryCategory.Fundamentals,
            "The reference point on an end effector whose position or pose is controlled and displayed by the robot system.",
            ["End effector", "Path", "Pose"]),
        Entry("Timeline", GlossaryCategory.Simulation,
            "An ordered view of simulation frames, commands, and state changes across simulated time.",
            ["Command sequence", "Playback", "Simulation timestep"]),
        Entry("Trajectory", GlossaryCategory.Motion,
            "A path combined with timing information, describing both where the robot moves and when it reaches each point.",
            ["Motion profile", "Path", "Playback"]),
        Entry("Velocity", GlossaryCategory.Motion,
            "The rate and direction of position change. Linear velocity uses distance per unit time, while angular velocity uses angle per unit time.",
            ["Acceleration", "Effective velocity", "Requested velocity"]),
        Entry("Velocity limit", GlossaryCategory.Safety,
            "The maximum permitted speed for an axis, joint, actuator, or coordinated movement. The slowest involved component may limit the whole move.",
            ["Effective velocity", "Requested velocity", "Robot profile"]),
        Entry("Wheelbase", GlossaryCategory.Fundamentals,
            "The distance between the left and right wheel contact paths of a differential-drive robot. It affects how wheel travel translates into heading change.",
            ["Differential drive", "Odometry"]),
        Entry("Workspace", GlossaryCategory.Fundamentals,
            "The set of positions or poses a robot can physically reach while respecting its geometry and configured limits.",
            ["Robot profile", "Safety limits", "Tool center point"])
    ];

    public static IReadOnlyList<GlossaryEntry> All { get; } =
        Array.AsReadOnly(Entries.OrderBy(entry => entry.Term, StringComparer.OrdinalIgnoreCase).ToArray());

    public static IReadOnlyList<GlossaryEntry> Search(
        string? query,
        GlossaryCategory? category = null)
    {
        var normalizedQuery = query?.Trim();

        return All
            .Where(entry => category is null || entry.Category == category)
            .Where(entry => string.IsNullOrWhiteSpace(normalizedQuery) || Matches(entry, normalizedQuery))
            .ToArray();
    }

    private static bool Matches(GlossaryEntry entry, string query) =>
        entry.Term.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        (entry.Acronym?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
        entry.Definition.Contains(query, StringComparison.OrdinalIgnoreCase) ||
        entry.RelatedTerms.Any(term => term.Contains(query, StringComparison.OrdinalIgnoreCase));

    private static GlossaryEntry Entry(
        string term,
        GlossaryCategory category,
        string definition,
        IReadOnlyList<string> relatedTerms) =>
        new(term, Acronym: null, category, definition, relatedTerms);

    private static GlossaryEntry Entry(
        string term,
        string acronym,
        GlossaryCategory category,
        string definition,
        IReadOnlyList<string> relatedTerms) =>
        new(term, acronym, category, definition, relatedTerms);
}
