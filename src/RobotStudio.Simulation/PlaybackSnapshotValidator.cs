namespace RobotStudio.Simulation;

public sealed class PlaybackSnapshotValidator
{
    public PlaybackSnapshotValidationResult Validate(CartesianPlaybackSnapshot? snapshot)
    {
        var errors = new List<string>();

        if (snapshot is null)
        {
            errors.Add("Snapshot is missing.");
            return new PlaybackSnapshotValidationResult(errors);
        }

        ValidateMetadata(snapshot.Metadata, errors);
        ValidateRequiredSections(snapshot, errors);
        ValidateCounts(snapshot, errors);
        ValidateMotionMetrics(snapshot, errors);
        ValidateCommandMetadata(snapshot, errors);
        ValidateCommandMotions(snapshot, errors);

        if (snapshot.TotalDuration < TimeSpan.Zero)
        {
            errors.Add("Snapshot total duration cannot be negative.");
        }

        return errors.Count == 0
            ? PlaybackSnapshotValidationResult.Valid
            : new PlaybackSnapshotValidationResult(errors);
    }

    private static void ValidateMetadata(
        PlaybackSnapshotMetadata? metadata,
        List<string> errors)
    {
        if (metadata is null)
        {
            errors.Add("Snapshot metadata is missing.");
            return;
        }

        if (metadata.FormatVersion is < 1 or > PlaybackSnapshotMetadata.CurrentCartesianFormatVersion)
        {
            errors.Add($"Unsupported snapshot format version: {metadata.FormatVersion}.");
        }

        if (!string.Equals(metadata.RobotFamily, "Cartesian", StringComparison.Ordinal))
        {
            errors.Add($"Unsupported robot family: {metadata.RobotFamily}.");
        }

        if (!string.Equals(metadata.DistanceUnit, "Millimeters", StringComparison.Ordinal))
        {
            errors.Add($"Unsupported distance unit: {metadata.DistanceUnit}.");
        }

        if (!string.Equals(metadata.TimeUnit, "Seconds", StringComparison.Ordinal))
        {
            errors.Add($"Unsupported time unit: {metadata.TimeUnit}.");
        }

        if (metadata.SampleIntervalMilliseconds <= 0)
        {
            errors.Add("Snapshot sample interval must be greater than zero.");
        }
    }

    private static void ValidateRequiredSections(
        CartesianPlaybackSnapshot snapshot,
        List<string> errors)
    {
        if (snapshot.WorkspaceBounds is null)
        {
            errors.Add("Snapshot workspace bounds are missing.");
        }

        if (snapshot.Viewport is null)
        {
            errors.Add("Snapshot viewport is missing.");
        }

        if (snapshot.Frames is null)
        {
            errors.Add("Snapshot frames are missing.");
        }

        if (snapshot.Poses is null)
        {
            errors.Add("Snapshot poses are missing.");
        }

        if (snapshot.SceneFrames is null)
        {
            errors.Add("Snapshot scene frames are missing.");
        }
    }

    private static void ValidateCounts(
        CartesianPlaybackSnapshot snapshot,
        List<string> errors)
    {
        if (snapshot.Frames is null || snapshot.Poses is null || snapshot.SceneFrames is null)
        {
            return;
        }

        if (snapshot.Frames.Count == 0)
        {
            errors.Add("Snapshot must contain at least one frame.");
        }

        if (snapshot.Frames.Count != snapshot.Poses.Count)
        {
            errors.Add("Snapshot frame count must match pose count.");
        }

        if (snapshot.Frames.Count != snapshot.SceneFrames.Count)
        {
            errors.Add("Snapshot frame count must match scene frame count.");
        }
    }

    private static void ValidateMotionMetrics(
        CartesianPlaybackSnapshot snapshot,
        List<string> errors)
    {
        if (snapshot.Metadata?.FormatVersion < 2 || snapshot.Frames is null)
        {
            return;
        }

        if (snapshot.Frames.Any(frame =>
            !double.IsFinite(frame.VelocityMillimetersPerSecond) ||
            frame.VelocityMillimetersPerSecond < 0))
        {
            errors.Add("Snapshot frame velocities must be finite and non-negative.");
        }

        if (snapshot.Frames.Any(frame =>
            !double.IsFinite(frame.AccelerationMillimetersPerSecondSquared)))
        {
            errors.Add("Snapshot frame accelerations must be finite.");
        }
    }

    private static void ValidateCommandMetadata(
        CartesianPlaybackSnapshot snapshot,
        List<string> errors)
    {
        if (snapshot.Metadata?.FormatVersion < 3 || snapshot.Frames is null)
        {
            return;
        }

        if (snapshot.Frames.Any(frame =>
            frame.RequestedVelocityMillimetersPerSecond is { } velocity &&
            (!double.IsFinite(velocity) || velocity <= 0)))
        {
            errors.Add("Snapshot requested velocities must be finite and greater than zero when present.");
        }

        if (snapshot.Frames.Any(frame => frame.RequestedWaitDuration < TimeSpan.Zero))
        {
            errors.Add("Snapshot requested wait durations cannot be negative.");
        }

        if (snapshot.Poses is null || snapshot.SceneFrames is null ||
            snapshot.Frames.Count != snapshot.Poses.Count ||
            snapshot.Frames.Count != snapshot.SceneFrames.Count)
        {
            return;
        }

        for (var index = 0; index < snapshot.Frames.Count; index++)
        {
            var frame = snapshot.Frames[index];
            var pose = snapshot.Poses[index];
            var sceneFrame = snapshot.SceneFrames[index];
            if (frame.RequestedVelocityMillimetersPerSecond != pose.RequestedVelocityMillimetersPerSecond ||
                frame.RequestedVelocityMillimetersPerSecond != sceneFrame.RequestedVelocityMillimetersPerSecond ||
                frame.RequestedWaitDuration != pose.RequestedWaitDuration ||
                frame.RequestedWaitDuration != sceneFrame.RequestedWaitDuration)
            {
                errors.Add($"Snapshot command metadata is inconsistent at frame {index}.");
                return;
            }
        }
    }

    private static void ValidateCommandMotions(
        CartesianPlaybackSnapshot snapshot,
        List<string> errors)
    {
        if (snapshot.Metadata?.FormatVersion < 4)
        {
            return;
        }

        if (snapshot.CommandMotions is null)
        {
            errors.Add("Snapshot command motion summaries are missing.");
            return;
        }

        if (snapshot.CommandMotions.GroupBy(motion => motion.CommandIndex).Any(group => group.Count() > 1))
        {
            errors.Add("Snapshot command motion indexes must be unique.");
        }

        foreach (var motion in snapshot.CommandMotions)
        {
            if (motion.CommandIndex < 0 || string.IsNullOrWhiteSpace(motion.CommandName))
            {
                errors.Add("Snapshot command motion identity is invalid.");
                return;
            }

            if (motion.InvolvedAxes is null ||
                !IsFiniteNonNegative(motion.DistanceMillimeters) ||
                !IsFiniteNonNegative(motion.VelocityLimitMillimetersPerSecond) ||
                !IsFiniteNonNegative(motion.PeakVelocityMillimetersPerSecond) ||
                !IsFiniteNonNegative(motion.AccelerationMillimetersPerSecondSquared) ||
                motion.AccelerationDuration < TimeSpan.Zero ||
                motion.ConstantVelocityDuration < TimeSpan.Zero ||
                motion.DecelerationDuration < TimeSpan.Zero ||
                motion.TotalDuration < TimeSpan.Zero)
            {
                errors.Add("Snapshot command motion metrics must be finite and non-negative.");
                return;
            }

            var phaseDuration =
                motion.AccelerationDuration +
                motion.ConstantVelocityDuration +
                motion.DecelerationDuration;
            if (Math.Abs((phaseDuration - motion.TotalDuration).Ticks) > 2)
            {
                errors.Add($"Snapshot command motion duration is inconsistent for command {motion.CommandIndex}.");
                return;
            }

            var isStationary = motion.ProfileShape == MotionProfileShape.Stationary;
            if (isStationary != (motion.DistanceMillimeters == 0 && motion.InvolvedAxes.Count == 0))
            {
                errors.Add($"Snapshot command motion shape is inconsistent for command {motion.CommandIndex}.");
                return;
            }
        }
    }

    private static bool IsFiniteNonNegative(double value) =>
        double.IsFinite(value) && value >= 0;
}
