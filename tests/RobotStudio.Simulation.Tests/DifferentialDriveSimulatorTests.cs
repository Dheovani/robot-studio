using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;
using RobotStudio.Domain.Mobile;

namespace RobotStudio.Simulation.Tests;

public sealed class DifferentialDriveSimulatorTests
{
    [Fact]
    public void Execute_WhenFootprintCrossesObstacle_ShouldFaultWithoutMovingOrAddingOdometry()
    {
        var environment = new PlanarSimulationEnvironment(
            [new PlanarObstacle("wall", 180, 220, 80, 120)]);
        var start = new DifferentialDrivePose(0, 100, 0);
        var result = new DifferentialDriveSimulator(environment).Execute(
            DifferentialDriveSimulationContext.Create(CreateProfile(), start),
            new RobotCommandSequence(
                [new DifferentialDriveMoveCommand(new DifferentialDrivePose(300, 100, 0))]));

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.Equal(start, result.FinalContext.CurrentPose);
        Assert.Equal(DifferentialDriveOdometry.Zero, result.FinalContext.Odometry);
        Assert.Equal(TimeSpan.Zero, result.FinalContext.ElapsedTime);
        var exception = Assert.IsType<PlanarPathObstructedException>(result.Failure);
        Assert.Equal("wall", exception.ObstacleId);
        Assert.Equal(110, exception.RobotPose.X, precision: 10);
        Assert.Equal(180, exception.ContactXMillimeters);
    }

    [Fact]
    public void Execute_WhenHomePathCrossesObstacle_ShouldFaultAtCurrentPose()
    {
        var environment = new PlanarSimulationEnvironment(
            [new PlanarObstacle("home-barrier", 180, 220, 80, 120)]);
        var start = new DifferentialDrivePose(300, 100, 90);
        var result = new DifferentialDriveSimulator(environment).Execute(
            DifferentialDriveSimulationContext.Create(CreateProfile(), start),
            new RobotCommandSequence([new HomeCommand()]));

        Assert.False(result.Succeeded);
        Assert.Equal(start, result.FinalContext.CurrentPose);
        Assert.IsType<PlanarPathObstructedException>(result.Failure);
    }

    [Fact]
    public void Execute_WhenFootprintAvoidsObstacle_ShouldCompleteNormally()
    {
        var environment = new PlanarSimulationEnvironment(
            [new PlanarObstacle("clear-wall", 180, 220, 250, 300)]);
        var target = new DifferentialDrivePose(300, 100, 90);
        var result = new DifferentialDriveSimulator(environment).Execute(
            DifferentialDriveSimulationContext.Create(
                CreateProfile(),
                new DifferentialDrivePose(0, 100, 0)),
            new RobotCommandSequence([new DifferentialDriveMoveCommand(target)]));

        Assert.True(result.Succeeded);
        Assert.Equal(target, result.FinalContext.CurrentPose);
        Assert.NotEqual(DifferentialDriveOdometry.Zero, result.FinalContext.Odometry);
    }

    [Fact]
    public void Execute_WhenResettingFault_ShouldPreservePoseOdometryAndElapsedTime()
    {
        var pose = new DifferentialDrivePose(100, 80, 30);
        var context = DifferentialDriveSimulationContext.Create(CreateProfile(), pose) with
        {
            State = RobotState.Faulted,
            ElapsedTime = TimeSpan.FromSeconds(2),
            Odometry = new DifferentialDriveOdometry(120, 120, 80, 80)
        };

        var result = new DifferentialDriveSimulator().Execute(
            context,
            new RobotCommandSequence([new ResetFaultCommand()]));

        Assert.True(result.Succeeded);
        Assert.Equal(RobotState.Idle, result.FinalContext.State);
        Assert.Equal(context.CurrentPose, result.FinalContext.CurrentPose);
        Assert.Equal(context.Odometry, result.FinalContext.Odometry);
        Assert.Equal(context.ElapsedTime, result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenCommandIsHome_ShouldMoveToOriginPose()
    {
        var simulator = new DifferentialDriveSimulator();
        var context = DifferentialDriveSimulationContext.Create(
            CreateProfile(),
            new DifferentialDrivePose(X: 100, Y: 80, HeadingDegrees: 90));
        var sequence = new RobotCommandSequence([new HomeCommand()]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0), result.FinalContext.CurrentPose);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.Zero);
    }

    [Fact]
    public void Execute_WhenCommandIsDifferentialDriveMove_ShouldMoveToTargetPose()
    {
        var simulator = new DifferentialDriveSimulator();
        var context = DifferentialDriveSimulationContext.Create(
            CreateProfile(),
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0));
        var target = new DifferentialDrivePose(X: 100, Y: 0, HeadingDegrees: 90);
        var sequence = new RobotCommandSequence([new DifferentialDriveMoveCommand(target)]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(target, result.FinalContext.CurrentPose);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.True(result.FinalContext.ElapsedTime > TimeSpan.Zero);
    }

    [Fact]
    public void Execute_WhenMoveHasRequestedVelocities_ShouldUseThemInDuration()
    {
        var simulator = new DifferentialDriveSimulator();
        var context = DifferentialDriveSimulationContext.Create(
            CreateProfile(),
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0));
        var target = new DifferentialDrivePose(X: 100, Y: 0, HeadingDegrees: 90);
        var sequence = new RobotCommandSequence(
        [
            new DifferentialDriveMoveCommand(
                target,
                requestedLinearVelocityMillimetersPerSecond: 50,
                requestedAngularVelocityDegreesPerSecond: 45)
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(TimeSpan.FromMilliseconds(4225), result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenCommandIsWait_ShouldAdvanceTimeWithoutMoving()
    {
        var simulator = new DifferentialDriveSimulator();
        var pose = new DifferentialDrivePose(X: 10, Y: 20, HeadingDegrees: 30);
        var context = DifferentialDriveSimulationContext.Create(CreateProfile(), pose);
        var sequence = new RobotCommandSequence([new WaitCommand(TimeSpan.FromMilliseconds(500))]);

        var result = simulator.Execute(context, sequence);

        Assert.True(result.Succeeded);
        Assert.Equal(pose, result.FinalContext.CurrentPose);
        Assert.Equal(RobotState.Completed, result.FinalContext.State);
        Assert.Equal(TimeSpan.FromMilliseconds(500), result.FinalContext.ElapsedTime);
    }

    [Fact]
    public void Execute_WhenSequenceHasMultipleCommands_ShouldRecordStateTransitionsInOrder()
    {
        var simulator = new DifferentialDriveSimulator();
        var context = DifferentialDriveSimulationContext.Create(
            CreateProfile(),
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0));
        var sequence = new RobotCommandSequence(
        [
            new HomeCommand(),
            new DifferentialDriveMoveCommand(new DifferentialDrivePose(X: 100, Y: 0, HeadingDegrees: 90)),
            new WaitCommand(TimeSpan.FromSeconds(1))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.Equal(
            [
                RobotState.Idle,
                RobotState.Homing,
                RobotState.Completed,
                RobotState.Moving,
                RobotState.Moving,
                RobotState.Completed,
                RobotState.Waiting,
                RobotState.Completed
            ],
            result.Timeline.Select(step => step.State));
    }

    [Fact]
    public void Execute_WhenMoveTranslatesAndRotates_ShouldRecordSequentialSegments()
    {
        var context = DifferentialDriveSimulationContext.Create(
            CreateProfile(),
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0));
        var sequence = new RobotCommandSequence(
        [
            new DifferentialDriveMoveCommand(
                new DifferentialDrivePose(X: 100, Y: 0, HeadingDegrees: 90),
                requestedLinearVelocityMillimetersPerSecond: 100,
                requestedAngularVelocityDegreesPerSecond: 90)
        ]);

        var result = new DifferentialDriveSimulator().Execute(context, sequence);

        Assert.Equal(new DifferentialDrivePose(100, 0, 0), result.Timeline[2].Pose);
        Assert.Equal(RobotState.Moving, result.Timeline[2].State);
        Assert.NotNull(result.Timeline[1].MotionProfile);
        Assert.NotNull(result.Timeline[2].MotionProfile);
        Assert.Equal(100 - (30 * Math.PI), result.FinalContext.Odometry.LeftWheelTravelMillimeters, precision: 6);
        Assert.Equal(100 + (30 * Math.PI), result.FinalContext.Odometry.RightWheelTravelMillimeters, precision: 6);
    }

    [Fact]
    public void Execute_WhenCommandFails_ShouldReturnFaultedResultAndPreserveLastPose()
    {
        var simulator = new DifferentialDriveSimulator();
        var context = DifferentialDriveSimulationContext.Create(
            CreateProfile(),
            new DifferentialDrivePose(X: 0, Y: 0, HeadingDegrees: 0));
        var lastValidPose = new DifferentialDrivePose(X: 100, Y: 0, HeadingDegrees: 0);
        var sequence = new RobotCommandSequence(
        [
            new DifferentialDriveMoveCommand(lastValidPose),
            new DifferentialDriveMoveCommand(new DifferentialDrivePose(X: 501, Y: 0, HeadingDegrees: 0))
        ]);

        var result = simulator.Execute(context, sequence);

        Assert.False(result.Succeeded);
        Assert.Equal(RobotState.Faulted, result.FinalContext.State);
        Assert.Equal(lastValidPose, result.FinalContext.CurrentPose);
        Assert.Equal(lastValidPose, result.Timeline[^1].Pose);
        Assert.IsType<PositionOutOfRangeException>(result.Failure);
        Assert.Equal(1, result.Timeline[^1].CommandIndex);
        Assert.Equal(nameof(DifferentialDriveMoveCommand), result.Timeline[^1].CommandName);
    }

    [Fact]
    public void Create_WhenInitialPoseIsOutsideProfile_ShouldThrow()
    {
        var profile = CreateProfile();

        Assert.Throws<PositionOutOfRangeException>(() =>
            DifferentialDriveSimulationContext.Create(
                profile,
                new DifferentialDrivePose(X: 501, Y: 0, HeadingDegrees: 0)));
    }

    private static DifferentialDriveProfile CreateProfile() =>
        new(
            minimumXMillimeters: 0,
            maximumXMillimeters: 500,
            minimumYMillimeters: 0,
            maximumYMillimeters: 400,
            wheelBaseMillimeters: 120,
            wheelRadiusMillimeters: 30,
            collisionRadiusMillimeters: 70,
            maximumLinearVelocityMillimetersPerSecond: 250,
            maximumAngularVelocityDegreesPerSecond: 180,
            maximumLinearAccelerationMillimetersPerSecondSquared: 500,
            maximumAngularAccelerationDegreesPerSecondSquared: 360);
}
