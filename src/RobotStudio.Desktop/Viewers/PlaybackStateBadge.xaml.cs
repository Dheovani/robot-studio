using System.Windows;
using System.Windows.Controls;
using RobotStudio.Domain;

namespace RobotStudio.Desktop.Viewers;

public partial class PlaybackStateBadge : UserControl
{
    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State),
        typeof(RobotState),
        typeof(PlaybackStateBadge),
        new PropertyMetadata(RobotState.Idle, StatePropertyChanged));

    public PlaybackStateBadge()
    {
        InitializeComponent();
        ApplyState(State);
    }

    public RobotState State
    {
        get => (RobotState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    private static void StatePropertyChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs eventArgs)
    {
        if (dependencyObject is PlaybackStateBadge badge && eventArgs.NewValue is RobotState state)
        {
            badge.ApplyState(state);
        }
    }

    private void ApplyState(RobotState state)
    {
        var presentation = state switch
        {
            RobotState.Moving => new StatePresentation(
                "Moving",
                "AccentBackgroundBrush",
                "PrimaryTextBrush",
                "The robot pose changes as the active motion command advances."),
            RobotState.Homing => new StatePresentation(
                "Homing",
                "SelectedItemBackgroundBrush",
                "SelectedItemTextBrush",
                "The robot is moving toward its configured home pose."),
            RobotState.Waiting => new StatePresentation(
                "Waiting · pose held",
                "EffectiveSeriesBrush",
                "HeaderBackgroundBrush",
                "The WAIT command keeps the robot pose fixed while simulated time advances."),
            RobotState.Completed => new StatePresentation(
                "Completed · final pose",
                "PositiveSeriesBrush",
                "HeaderBackgroundBrush",
                "Playback reached the final pose; later frames do not contain additional movement."),
            RobotState.Faulted => new StatePresentation(
                "Faulted",
                "NegativeSeriesBrush",
                "HeaderBackgroundBrush",
                "Simulation stopped because the active command produced a fault."),
            _ => new StatePresentation(
                "Idle · pose held",
                "TagBackgroundBrush",
                "SecondaryTextBrush",
                "The robot is idle, so its pose remains fixed until another command begins.")
        };

        BadgeText.Text = presentation.Label;
        BadgeText.SetResourceReference(TextBlock.ForegroundProperty, presentation.ForegroundResourceKey);
        BadgeSurface.SetResourceReference(Border.BackgroundProperty, presentation.BackgroundResourceKey);
        ToolTip = presentation.ToolTip;
    }

    private sealed record StatePresentation(
        string Label,
        string BackgroundResourceKey,
        string ForegroundResourceKey,
        string ToolTip);
}
