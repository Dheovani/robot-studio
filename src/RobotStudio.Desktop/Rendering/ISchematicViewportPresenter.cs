namespace RobotStudio.Desktop.Rendering;

internal interface ISchematicViewportPresenter
{
    void Present(SchematicViewportScene scene);

    void Clear();
}
