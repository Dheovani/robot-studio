using RobotStudio.Domain.Articulated;
using RobotStudio.Domain.Cartesian;
using RobotStudio.Domain.Commands;

namespace RobotStudio.Scripting.Tests;

public sealed class GCodeRobotMappingTests
{
    [Fact]
    public void Catalog_ShouldDescribeEveryCurrentRobotTargetExactlyOnce()
    {
        var expectedTargets = Enum.GetValues<GCodeRobotTarget>();

        Assert.Equal(expectedTargets.Length, GCodeRobotMappingCatalog.All.Count);
        Assert.All(
            expectedTargets,
            target => Assert.Single(
                GCodeRobotMappingCatalog.All,
                mapping => mapping.Target == target));
    }

    [Theory]
    [InlineData(GCodeRobotTarget.CartesianRobot)]
    [InlineData(GCodeRobotTarget.XYPlotter)]
    [InlineData(GCodeRobotTarget.ScaraRobot)]
    [InlineData(GCodeRobotTarget.SimpleArticulatedArm)]
    public void Catalog_WhenMappingIsImplemented_ShouldMarkOnlyCartesianFamilyAvailable(
        GCodeRobotTarget target)
    {
        var mapping = GCodeRobotMappingCatalog.Get(target);

        Assert.Equal(GCodeRobotMappingStatus.Available, mapping.Status);
        Assert.NotEmpty(mapping.ToolSpaceWords);
    }

    [Theory]
    [InlineData(GCodeRobotTarget.DeltaRobot)]
    [InlineData(GCodeRobotTarget.IndustrialArm6Dof)]
    public void Catalog_WhenToolSpaceKinematicsAreRequired_ShouldMarkMappingPlanned(
        GCodeRobotTarget target)
    {
        var mapping = GCodeRobotMappingCatalog.Get(target);

        Assert.Equal(GCodeRobotMappingStatus.Planned, mapping.Status);
        Assert.Contains("Requires", mapping.Rationale);
    }

    [Theory]
    [InlineData(GCodeRobotTarget.DifferentialDriveRobot)]
    [InlineData(GCodeRobotTarget.Drone)]
    public void Catalog_WhenCncSemanticsDoNotFit_ShouldMarkMappingNotApplicable(
        GCodeRobotTarget target)
    {
        var mapping = GCodeRobotMappingCatalog.Get(target);

        Assert.Equal(GCodeRobotMappingStatus.NotApplicable, mapping.Status);
        Assert.Empty(mapping.ToolSpaceWords);
    }

    [Fact]
    public void CartesianMapper_WhenContextUsesJointCoordinates_ShouldRejectImplicitJointMapping()
    {
        var context = new RobotScriptParseContext(
            new ScaraJointPosition(ShoulderDegrees: 0, ElbowDegrees: 0));

        var exception = Assert.Throws<ArgumentException>(() =>
            new GCodeParser().Compile("G1 X10 Y20 Z0", context));

        Assert.Contains("tool-space coordinates", exception.Message);
        Assert.Contains(nameof(ScaraJointPosition), exception.Message);
    }

    [Fact]
    public void Parser_WhenCustomMapperIsProvided_ShouldDelegateSemanticProgram()
    {
        var mapper = new RecordingMapper();
        var parser = new GCodeParser(mapper);

        var compilation = parser.Compile("G21\nG1 X10 Y20 Z5");

        Assert.NotNull(mapper.Program);
        Assert.Collection(
            mapper.Program.Instructions,
            instruction => Assert.IsType<GCodeUnitInstruction>(instruction),
            instruction => Assert.IsType<GCodeLinearMoveInstruction>(instruction));
        Assert.IsType<HomeCommand>(Assert.Single(compilation.Commands.Commands));
    }

    private sealed class RecordingMapper : IGCodeCommandMapper
    {
        public GCodeProgram Program { get; private set; } = null!;

        public RobotScriptCompilation Map(
            GCodeProgram program,
            RobotScriptParseContext? context = null)
        {
            Program = program;
            return new RobotScriptCompilation(
            [
                new RobotScriptCommandStatement(
                    new HomeCommand(new RobotCommandSource(1, "G28")))
            ]);
        }
    }
}
