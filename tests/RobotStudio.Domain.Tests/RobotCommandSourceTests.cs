using RobotStudio.Domain.Commands;

namespace RobotStudio.Domain.Tests;

public sealed class RobotCommandSourceTests
{
    [Fact]
    public void Constructor_WhenValuesAreValid_ShouldCreateSource()
    {
        var source = new RobotCommandSource(2, "MOVE X=10 Y=20 Z=5");

        Assert.Equal(2, source.LineNumber);
        Assert.Equal("MOVE X=10 Y=20 Z=5", source.Text);
    }

    [Fact]
    public void Constructor_WhenLineNumberIsNotPositive_ShouldThrow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RobotCommandSource(0, "HOME"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_WhenTextIsBlank_ShouldThrow(string text)
    {
        Assert.Throws<ArgumentException>(() =>
            new RobotCommandSource(1, text));
    }
}
