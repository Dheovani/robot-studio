using RobotStudio.Desktop.Showcases;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class MechanicalTeachingViewCatalogTests
{
    [Fact]
    public void Options_ShouldExposeAllMechanicalViewLayers()
    {
        Assert.Equal(
            [
                MechanicalTeachingViewMode.Assembled,
                MechanicalTeachingViewMode.DriveSystem,
                MechanicalTeachingViewMode.MotionAxes
            ],
            MechanicalTeachingViewCatalog.Options.Select(option => option.Mode));
        Assert.All(
            MechanicalTeachingViewCatalog.Options,
            option => Assert.False(string.IsNullOrWhiteSpace(option.Description)));
    }

    [Fact]
    public void MotionAxes_ShouldExposeThreeNonZeroDirectionalGuides()
    {
        Assert.Equal(
            [MechanicalMotionAxis.X, MechanicalMotionAxis.Y, MechanicalMotionAxis.Z],
            MechanicalTeachingViewCatalog.MotionAxes.Select(guide => guide.Axis));
        Assert.All(
            MechanicalTeachingViewCatalog.MotionAxes,
            guide => Assert.NotEqual(guide.Start, guide.End));
        Assert.Equal(
            new RobotPartId("z-gantry"),
            MechanicalTeachingViewCatalog.MotionAxes.Single(guide => guide.Axis == MechanicalMotionAxis.X).AttachedPartId);
    }

    [Theory]
    [InlineData(RobotPartKind.Base)]
    [InlineData(RobotPartKind.Structure)]
    [InlineData(RobotPartKind.Carriage)]
    [InlineData(RobotPartKind.Controller)]
    public void ShouldGhost_WhenPartCanObscureDriveComponents_ShouldReturnTrue(RobotPartKind kind)
    {
        Assert.True(MechanicalTeachingViewCatalog.ShouldGhost(kind));
    }

    [Theory]
    [InlineData(RobotPartKind.Motor)]
    [InlineData(RobotPartKind.Transmission)]
    [InlineData(RobotPartKind.Rail)]
    [InlineData(RobotPartKind.Tool)]
    public void ShouldGhost_WhenPartExplainsTheDriveSystem_ShouldReturnFalse(RobotPartKind kind)
    {
        Assert.False(MechanicalTeachingViewCatalog.ShouldGhost(kind));
    }
}
