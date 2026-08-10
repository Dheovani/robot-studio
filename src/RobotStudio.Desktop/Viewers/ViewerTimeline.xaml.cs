using System.Windows;
using System.Windows.Controls;

namespace RobotStudio.Desktop.Viewers;

public partial class ViewerTimeline : UserControl
{
    public ViewerTimeline()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler? PreviousRequested;

    public event RoutedEventHandler? NextRequested;

    public event RoutedPropertyChangedEventHandler<double>? ValueChanged;

    public double Maximum
    {
        get => FrameSlider.Maximum;
        set => FrameSlider.Maximum = value;
    }

    public double Value
    {
        get => FrameSlider.Value;
        set => FrameSlider.Value = value;
    }

    public double TickFrequency
    {
        get => FrameSlider.TickFrequency;
        set => FrameSlider.TickFrequency = value;
    }

    public string Status
    {
        get => StatusTextBlock.Text;
        set => StatusTextBlock.Text = value;
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e) =>
        PreviousRequested?.Invoke(this, e);

    private void NextButton_Click(object sender, RoutedEventArgs e) =>
        NextRequested?.Invoke(this, e);

    private void FrameSlider_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e) =>
        ValueChanged?.Invoke(this, e);
}
