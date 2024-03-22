namespace TuringMachineEmulator;

public class Command
{
    private string currentState;
    public string CurrentState => currentState;

    private char currentSymbol;
    public char CurrentSymbol => currentSymbol;

    private char newSymbol;
    public char NewSymbol => newSymbol;

    private TuringMachine.Direction direction;
    public TuringMachine.Direction Direction => direction;

    private string newState;
    public string NewState => newState;

    public Command() { }

    public Command(string currentState, char currentSymbol, char newSymbol, TuringMachine.Direction direction, string newState)
    {
        this.currentState = currentState;
        this.currentSymbol = currentSymbol;
        this.newSymbol = newSymbol;
        this.direction = direction;
        this.newState = newState;
    }

    public override string ToString() => $"{currentState} {currentSymbol} {newSymbol} {direction} {newState}";
}
