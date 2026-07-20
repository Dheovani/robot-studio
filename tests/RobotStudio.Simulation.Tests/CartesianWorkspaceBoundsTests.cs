namespace RobotStudio.Simulation.Tests;

public sealed class CartesianWorkspaceBoundsTests
{
    [Fact]
    public void FromProfile_ShouldUseAxisLimitsAsVisualBounds()
    {
        var profile = CreateProfile();

        var bounds = CartesianWorkspaceBounds.FromProfile(profile);

        Assert.Equal(new VisualVector3(0, -50, 10), bounds.Minimum);
        Assert.Equal(new VisualVector3(300, 200, 160), bounds.Maximum);
    }

    [Fact]
    public void Size_ShouldReturnAxisRanges()
    {
        var bounds = CartesianWorkspaceBounds.FromProfile(CreateProfile());

        Assert.Equal(new VisualVector3(300, 250, 150), bounds.Size);
    }

    [Fact]
    public void Center_ShouldReturnMiddlePointOfWorkspace()
    {
        var bounds = CartesianWorkspaceBounds.FromProfile(CreateProfile());

        Assert.Equal(new VisualVector3(150, 75, 85), bounds.Center);
    }

    [Fact]
    public void Contains_WhenPositionIsInsideWorkspace_ShouldReturnTrue()
    {
        var bounds = CartesianWorkspaceBounds.FromProfile(CreateProfile());

        Assert.True(bounds.Contains(new VisualVector3(150, 75, 85)));
    }

    [Fact]
    public void Contains_WhenPositionIsOnWorkspaceBoundary_ShouldReturnTrue()
    {
        var bounds = CartesianWorkspaceBounds.FromProfile(CreateProfile());

        Assert.True(bounds.Contains(new VisualVector3(300, 200, 160)));
    }

    [Fact]
    public void Contains_WhenPositionIsOutsideWorkspace_ShouldReturnFalse()
    {
        var bounds = CartesianWorkspaceBounds.FromProfile(CreateProfile());

        Assert.False(bounds.Contains(new VisualVector3(301, 200, 160)));
    }

    [Fact]
    public void FromProfile_WhenProfileIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CartesianWorkspaceBounds.FromProfile(null!));
    }

    private static CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, -50, 200, 100, 200),
            new Axis(AxisId.Z, 10, 160, 80, 160));
}
