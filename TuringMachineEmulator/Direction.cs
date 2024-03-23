namespace TuringMachineEmulator;

public enum Direction
{
    Left,
    Right,
}

public static class DirectionExtensions
{
    public static int Delta(this Direction direction) => direction == Direction.Left ? -1 : 1;
}