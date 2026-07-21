namespace RobotStudio.Simulation.Tests;

public sealed class CartesianViewportPlannerTests
{
    [Fact]
    public void Plan_ShouldTargetWorkspaceCenter()
    {
        var planner = new CartesianViewportPlanner();
        var bounds = CreateWorkspaceBounds();

        var viewport = planner.Plan(bounds);

        Assert.Equal(bounds.Center, viewport.Target);
    }

    [Fact]
    public void Plan_ShouldPlaceCameraAtDiagonalViewpoint()
    {
        var planner = new CartesianViewportPlanner();
        var bounds = CreateWorkspaceBounds();

        var viewport = planner.Plan(bounds);

        Assert.Equal(new VisualVector3(810, -560, 735), viewport.CameraPosition);
    }

    [Fact]
    public void Plan_ShouldUseZAxisAsUpDirection()
    {
        var planner = new CartesianViewportPlanner();

        var viewport = planner.Plan(CreateWorkspaceBounds());

        Assert.Equal(new VisualVector3(0, 0, 1), viewport.Up);
    }

    [Fact]
    public void Plan_ShouldCreatePositiveClipDistances()
    {
        var planner = new CartesianViewportPlanner();

        var viewport = planner.Plan(CreateWorkspaceBounds());

        Assert.Equal(1, viewport.NearClipMillimeters);
        Assert.Equal(2640, viewport.FarClipMillimeters);
        Assert.True(viewport.FarClipMillimeters > viewport.NearClipMillimeters);
    }

    [Fact]
    public void Plan_WhenWorkspaceHasZeroSize_ShouldStillCreateUsableCamera()
    {
        var planner = new CartesianViewportPlanner();
        var bounds = new CartesianWorkspaceBounds(
            new VisualVector3(10, 20, 30),
            new VisualVector3(10, 20, 30));

        var viewport = planner.Plan(bounds);

        Assert.Equal(new VisualVector3(12.2, 17.8, 32.2), viewport.CameraPosition);
        Assert.Equal(8.8, viewport.FarClipMillimeters);
    }

    [Fact]
    public void Plan_WhenWorkspaceBoundsIsNull_ShouldThrow()
    {
        var planner = new CartesianViewportPlanner();

        Assert.Throws<ArgumentNullException>(() => planner.Plan(null!));
    }

    private static CartesianWorkspaceBounds CreateWorkspaceBounds() =>
        new(
            new VisualVector3(0, 0, 0),
            new VisualVector3(300, 200, 150));
}
