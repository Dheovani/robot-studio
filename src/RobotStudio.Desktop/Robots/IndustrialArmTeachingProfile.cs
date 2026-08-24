using RobotStudio.Domain.Articulated;

namespace RobotStudio.Desktop.Robots;

public static class IndustrialArmTeachingProfile
{
    public static IndustrialArmRobotProfile Create() =>
        new(
            baseHeightMillimeters: 110,
            upperArmLengthMillimeters: 180,
            forearmLengthMillimeters: 140,
            wristLengthMillimeters: 80,
            linkCollisionRadiusMillimeters: 12,
            joints:
            [
                new(IndustrialArmJointId.J1Base, -180, 180, 120, 240),
                new(IndustrialArmJointId.J2Shoulder, -120, 120, 100, 200),
                new(IndustrialArmJointId.J3Elbow, -150, 150, 90, 180),
                new(IndustrialArmJointId.J4WristRoll, -180, 180, 160, 320),
                new(IndustrialArmJointId.J5WristPitch, -120, 120, 110, 220),
                new(IndustrialArmJointId.J6ToolRoll, -360, 360, 200, 400)
            ]);
}
