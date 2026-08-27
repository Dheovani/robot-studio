using System.Windows;

namespace RobotStudio.Desktop.Showcases;

public partial class MechanicalShowcaseView
{
    private void MechanicalShowcaseView_Unloaded(object sender, RoutedEventArgs e)
    {
        Dispose();
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        timer.Stop();
        timer.Tick -= Timer_Tick;
        stopwatch.Stop();
        Unloaded -= MechanicalShowcaseView_Unloaded;
        ShowcaseViewport.MouseDown3D -= ShowcaseViewport_MouseDown3D;
        importedSceneHost?.Clear(detachChildren: true);
        importedScene?.Dispose();
        importedScene = null;
        importedSceneHost = null;
        ShowcaseViewport.EffectsManager = null;
        ShowcaseViewport.Camera = null;
        effectsManager.Dispose();
        isDisposed = true;
        GC.SuppressFinalize(this);
    }
}
