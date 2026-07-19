using RobotStudio.Domain;
using RobotStudio.Domain.Commands;
using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class RobotCommandValidatorTests
{
    [Fact]
    public void Validate_WhenCommandIsNull_ShouldThrow()
    {
        var profile = CreateProfile();

        Assert.Throws<ArgumentNullException>(() => RobotCommandValidator.Validate(null!, profile));
    }

    [Fact]
    public void Validate_WhenProfileIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => RobotCommandValidator.Validate(new HomeCommand(), null!));
    }

    [Fact]
    public void Validate_WhenCommandIsHome_ShouldNotThrow()
    {
        var profile = CreateProfile();

        var exception = Record.Exception(() => RobotCommandValidator.Validate(new HomeCommand(), profile));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenCommandIsWait_ShouldNotThrow()
    {
        var profile = CreateProfile();

        var exception = Record.Exception(() =>
            RobotCommandValidator.Validate(new WaitCommand(TimeSpan.FromSeconds(1)), profile));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenMoveTargetIsInsideLimits_ShouldNotThrow()
    {
        var profile = CreateProfile();
        var command = new MoveToCommand(new CartesianPosition(X: 10, Y: 20, Z: 30));

        var exception = Record.Exception(() => RobotCommandValidator.Validate(command, profile));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WhenMoveTargetIsOutsideLimits_ShouldThrow()
    {
        var profile = CreateProfile();
        var command = new MoveToCommand(new CartesianPosition(X: 301, Y: 20, Z: 30));

        Assert.Throws<PositionOutOfRangeException>(() => RobotCommandValidator.Validate(command, profile));
    }

    private static RobotProfile CreateProfile() =>
        RobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120),
            new Axis(AxisId.Y, 0, 200, 100),
            new Axis(AxisId.Z, 0, 150, 80));
}
