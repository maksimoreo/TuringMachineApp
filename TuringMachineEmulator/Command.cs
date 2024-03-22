namespace TuringMachineEmulator;

public record class Command(
    string CurrentState,
    char CurrentSymbol,
    char NewSymbol,
    TuringMachine.Direction Direction,
    string NewState)
{
    public override string ToString() => $"{CurrentState} {CurrentSymbol} {NewSymbol} {Direction} {NewState}";
}