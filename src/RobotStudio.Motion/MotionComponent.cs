namespace RobotStudio.Motion;

public readonly record struct MotionComponent(string Name)
{
    public override string ToString() => Name;
}
