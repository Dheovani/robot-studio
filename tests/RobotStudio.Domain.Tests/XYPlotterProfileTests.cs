using RobotStudio.Domain.Exceptions;

namespace RobotStudio.Domain.Tests;

public sealed class XYPlotterProfileTests
{
    [Fact]
    public void ValidatePosition_DoesNotThrow_WhenPositionIsInsideAxisLimits()
    {
        var profile = CreateProfile();
        var position = new XYPlotterPosition(X: 120, Y: 80);

        var exception = Record.Exception(() => profile.ValidatePosition(position));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidatePosition_Throws_WhenXIsOutsideAxisLimits()
    {
        var profile = CreateProfile();
        var position = new XYPlotterPosition(X: 301, Y: 80);

        var exception = Assert.Throws<PositionOutOfRangeException>(() => profile.ValidatePosition(position));

        Assert.Equal(AxisId.X, exception.Axis);
    }

    [Fact]
    public void GetAxis_Throws_WhenZAxisIsRequested()
    {
        var profile = CreateProfile();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => profile.GetAxis(AxisId.Z));

        Assert.Contains("does not contain a Z axis", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ToCartesianProfile_ShouldCreateThinDrawingPlaneForVisualization()
    {
        var profile = CreateProfile();

        var cartesianProfile = profile.ToCartesianProfile(drawingPlaneZMillimeters: 0);

        Assert.Equal(0, cartesianProfile.ZAxis.MinimumMillimeters);
        Assert.Equal(1, cartesianProfile.ZAxis.MaximumMillimeters);
    }

    private static XYPlotterProfile CreateProfile() =>
        XYPlotterProfile.Create(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200));
}
