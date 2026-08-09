using System.Text.Json;
using System.Text.Json.Serialization;

namespace RobotStudio.Simulation.Tests;

public sealed class PlaybackSnapshotCompatibilityTests
{
    [Fact]
    public void Deserialize_WhenVersionOneFrameOmitsMotionMetrics_ShouldUseCompatibleDefaults()
    {
        const string json = """
            {
              "Time": "00:00:01",
              "State": "Moving",
              "Position": {
                "XMillimeters": 10,
                "YMillimeters": 20,
                "ZMillimeters": 30
              },
              "CommandIndex": 0,
              "CommandName": "MoveToCommand",
              "CommandSource": null
            }
            """;
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var frame = JsonSerializer.Deserialize<RobotVisualState>(json, options);

        Assert.NotNull(frame);
        Assert.Equal(0, frame.VelocityMillimetersPerSecond);
        Assert.Equal(0, frame.AccelerationMillimetersPerSecondSquared);
        Assert.Null(frame.MotionProfilePhase);
    }
}
