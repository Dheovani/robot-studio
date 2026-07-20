namespace RobotStudio.Domain;

public interface IRobotProfile<in TPosition>
    where TPosition : IRobotPosition
{
    void ValidatePosition(TPosition position);
}
