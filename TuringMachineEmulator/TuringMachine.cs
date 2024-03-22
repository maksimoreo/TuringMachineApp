namespace TuringMachineEmulator;

public class TuringMachine
{
    public enum Direction { Left, Right };

    public enum Status { Ready, NoInstruction, TapeBorder }
    public Status status { get; private set; } = Status.Ready;

    // Information about current state
    private List<char> tape;
    public string Tape { get => new(tape.ToArray()); set => tape = [.. value]; }
    public string CurrentState { get; set; }
    public int CurrentPosition { get; set; }

    private int steps = 0;
    public int Steps => steps;

    public char CurrentSymbol
    {
        get
        {
            for (int i = tape.Count; i < CurrentPosition + 1; i++)
                tape.Add('\0');
            return tape[CurrentPosition];
        }
        private set
        {
            for (int i = tape.Count; i < CurrentPosition + 1; i++)
                tape.Add('\0');
            tape[CurrentPosition] = value;
        }
    }

    public List<Command> commands = [];

    public bool Step()
    {
        if (status == Status.Ready)
        {
            // Find command with current state and symbol
            Command? command = commands.Find(x => x.CurrentState == CurrentState && x.CurrentSymbol == CurrentSymbol);

            // If found the command...
            if (command != null)
            {

                // Execute command
                CurrentSymbol = command.NewSymbol;
                CurrentState = command.NewState;
                int newPosition = CurrentPosition + (command.Direction == Direction.Left ? -1 : 1);
                if (newPosition >= 0 && newPosition < tape.Count)
                {
                    CurrentPosition = newPosition;
                }
                else
                {
                    status = Status.TapeBorder;
                    return false;
                }
                steps++;

                return true;
            }
            else // Halt
            {
                status = Status.NoInstruction;
                return false;
            }
        }
        else
            return false;
    }
}
