using RobotStudio.Desktop.Showcases;
using RobotStudio.Visualization;

namespace RobotStudio.Desktop.Tests;

public sealed class MechanicalTeachingViewCatalogTests
{
    [Fact]
    public void Options_ShouldExposeAssembledAndDriveSystemViews()
    {
        Assert.Equal(
            [MechanicalTeachingViewMode.Assembled, MechanicalTeachingViewMode.DriveSystem],
            MechanicalTeachingViewCatalog.Options.Select(option => option.Mode));
        Assert.All(
            MechanicalTeachingViewCatalog.Options,
            option => Assert.False(string.IsNullOrWhiteSpace(option.Description)));
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
