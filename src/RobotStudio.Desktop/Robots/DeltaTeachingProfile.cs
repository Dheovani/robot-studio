using RobotStudio.Domain.Parallel;

namespace RobotStudio.Desktop.Robots;

public static class DeltaTeachingProfile
{
    public static DeltaRobotProfile Create() =>
        new(
            baseRadiusMillimeters: 170,
            toolZOffsetMillimeters: 60,
            movingComponentCollisionRadiusMillimeters: 14,
            actuatorA: new DeltaActuator(
                DeltaActuatorId.A,
                minimumMillimeters: 0,
                maximumMillimeters: 120,
                maximumVelocityMillimetersPerSecond: 110,
                maximumAccelerationMillimetersPerSecondSquared: 220),
            actuatorB: new DeltaActuator(
                DeltaActuatorId.B,
                minimumMillimeters: 0,
                maximumMillimeters: 120,
                maximumVelocityMillimetersPerSecond: 100,
                maximumAccelerationMillimetersPerSecondSquared: 200),
            actuatorC: new DeltaActuator(
                DeltaActuatorId.C,
                minimumMillimeters: 0,
                maximumMillimeters: 120,
                maximumVelocityMillimetersPerSecond: 90,
                maximumAccelerationMillimetersPerSecondSquared: 180));
}
