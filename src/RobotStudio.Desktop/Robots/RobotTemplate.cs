namespace RobotStudio.Desktop.Robots;

public sealed record RobotTemplate(
    string Id,
    string Name,
    RobotFamilyDescriptor Family,
    RobotAvailabilityStatus Status,
    RobotComplexityLevel Complexity,
    string Description,
    IReadOnlyList<RobotCapability> Capabilities,
    RobotViewerDescriptor Viewer);
