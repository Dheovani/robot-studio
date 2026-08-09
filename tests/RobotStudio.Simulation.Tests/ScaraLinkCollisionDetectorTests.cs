using RobotStudio.Domain.Articulated;

namespace RobotStudio.Simulation.Tests;

public sealed class ScaraLinkCollisionDetectorTests
{
    [Fact]
    public void FindFirstCollision_WhenFirstLinkOccupiesObstacle_ShouldIdentifyFirstLink()
    {
        var collision = ScaraLinkCollisionDetector.FindFirstCollision(
            new ScaraJointPosition(0, 0),
            new ScaraJointPosition(0, 0),
            CreateProfile(),
            new PlanarSimulationEnvironment([new PlanarObstacle("fixture", 80, 100, -5, 5)]));

        Assert.NotNull(collision);
        Assert.Equal(ScaraLinkId.FirstLink, collision.Link);
        Assert.Equal(0, collision.TrajectoryFraction);
    }

    [Fact]
    public void FindFirstCollision_WhenSecondLinkOccupiesObstacle_ShouldIdentifySecondLink()
    {
        var collision = ScaraLinkCollisionDetector.FindFirstCollision(
            new ScaraJointPosition(0, 0),
            new ScaraJointPosition(0, 0),
            CreateProfile(),
            new PlanarSimulationEnvironment([new PlanarObstacle("fixture", 220, 240, -5, 5)]));

        Assert.NotNull(collision);
        Assert.Equal(ScaraLinkId.SecondLink, collision.Link);
    }

    [Fact]
    public void FindFirstCollision_WhenMovingLinkSweepsThroughObstacle_ShouldReturnIntermediateJoints()
    {
        var collision = ScaraLinkCollisionDetector.FindFirstCollision(
            new ScaraJointPosition(0, 0),
            new ScaraJointPosition(30, 0),
            CreateProfile(),
            new PlanarSimulationEnvironment([new PlanarObstacle("fixture", 80, 100, 20, 30)]));

        Assert.NotNull(collision);
        Assert.Equal(ScaraLinkId.FirstLink, collision.Link);
        Assert.InRange(collision.TrajectoryFraction, 0.01, 0.99);
        Assert.InRange(collision.Joints.ShoulderDegrees, 0.01, 29.99);
    }

    [Fact]
    public void FindFirstCollision_WhenMotionIsClear_ShouldReturnNull()
    {
        var collision = ScaraLinkCollisionDetector.FindFirstCollision(
            new ScaraJointPosition(0, 0),
            new ScaraJointPosition(30, 20),
            CreateProfile(),
            new PlanarSimulationEnvironment([new PlanarObstacle("clear", -300, -250, -300, -250)]));

        Assert.Null(collision);
    }

    [Fact]
    public void FindFirstCollision_WhenSamplingStepIsInvalid_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ScaraLinkCollisionDetector.FindFirstCollision(
                new ScaraJointPosition(0, 0),
                new ScaraJointPosition(30, 0),
                CreateProfile(),
                PlanarSimulationEnvironment.Empty,
                maximumJointStepDegrees: 0));
    }

    private static ScaraRobotProfile CreateProfile() =>
        new(
            firstLinkLengthMillimeters: 180,
            secondLinkLengthMillimeters: 120,
            linkCollisionRadiusMillimeters: 12,
            shoulderJoint: new ScaraJoint(ScaraJointId.Shoulder, -180, 180, 120, 240),
            elbowJoint: new ScaraJoint(ScaraJointId.Elbow, -150, 150, 100, 200));
}
