namespace TuringMachineEmulator;

public record class Command(
    string CurrentState,
    char CurrentSymbol,
    char NewSymbol,
    TuringMachine.Direction Direction,
    string NewState)
{
    public override string ToString()
    {
        string directionText = Direction == TuringMachine.Direction.Left ? "L" : "R";
        return $"{CurrentState} {CurrentSymbol} {NewSymbol} {directionText} {NewState}";
    }
}