using RobotStudio.Desktop.Profiles;
using RobotStudio.Domain.Cartesian;

namespace RobotStudio.Desktop.Tests;

public sealed class CartesianProfileInputTests
{
    [Fact]
    public void FromProfile_WhenRecreated_ShouldPreserveAllAxisValues()
    {
        var original = CreateProfile();

        var recreated = CartesianProfileInput.FromProfile(original).CreateProfile();

        Assert.Equal(original.XAxis, recreated.XAxis);
        Assert.Equal(original.YAxis, recreated.YAxis);
        Assert.Equal(original.ZAxis, recreated.ZAxis);
    }

    [Fact]
    public void CreateProfile_WhenValuesUseInvariantDecimals_ShouldCreateProfile()
    {
        var input = CreateValidInput() with
        {
            XMaximum = "320.5",
            ZMaximumVelocity = "75.25"
        };

        var profile = input.CreateProfile();

        Assert.Equal(320.5, profile.XAxis.MaximumMillimeters);
        Assert.Equal(75.25, profile.ZAxis.MaximumVelocityMillimetersPerSecond);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-number")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void CreateProfile_WhenValueIsNotFinite_ShouldExplainField(string value)
    {
        var input = CreateValidInput() with { YMaximumVelocity = value };

        var exception = Assert.Throws<ArgumentException>(input.CreateProfile);

        Assert.Contains("Y maximum velocity must be a finite number", exception.Message);
    }

    [Fact]
    public void CreateProfile_WhenMaximumDoesNotExceedMinimum_ShouldUseDomainValidation()
    {
        var input = CreateValidInput() with
        {
            XMinimum = "100",
            XMaximum = "100"
        };

        var exception = Assert.Throws<ArgumentException>(input.CreateProfile);

        Assert.Contains("maximum limit must be greater", exception.Message);
    }

    private static CartesianProfileInput CreateValidInput() =>
        CartesianProfileInput.FromProfile(CreateProfile());

    private static CartesianRobotProfile CreateProfile() =>
        CartesianRobotProfile.CreateCartesian(
            new Axis(AxisId.X, 0, 300, 120, 240),
            new Axis(AxisId.Y, 0, 200, 100, 200),
            new Axis(AxisId.Z, 0, 150, 80, 160));
}
