namespace TuringMachineEmulator;

public class TuringMachine
{
    public enum Direction { L, R }; // Left, Right

    public enum Status { Ready, NoInstruction, TapeBorder }
    public Status status { get; private set; } = Status.Ready;

    // Information about current state
    private List<char> tape;
    public string Tape { get => new string(tape.ToArray()); set => tape = value.ToList(); }
    private string currentState;
    public string CurrentState { get => currentState; set => currentState = value; }

    private int currentPosition;
    public int CurrentPosition { get => currentPosition; set => currentPosition = value; }

    private int steps = 0;
    public int Steps => steps;

    public char currentSymbol
    {
        get
        {
            for (int i = tape.Count; i < currentPosition + 1; i++)
                tape.Add('\0');
            return tape[currentPosition];
        }
        private set
        {
            for (int i = tape.Count; i < currentPosition + 1; i++)
                tape.Add('\0');
            tape[currentPosition] = value;
        }
    }

    public List<Command> commands = new List<Command>();

    public bool Step()
    {
        if (status == Status.Ready)
        {
            // Find command with current state and symbol
            Command command = commands.Find(
                x => (x.CurrentState == currentState && x.CurrentSymbol == currentSymbol)
                );

            // If found the command...
            if (command != null)
            {

                // Execute command
                currentSymbol = command.NewSymbol;
                currentState = command.NewState;
                int newPosition = currentPosition + (command.Direction == Direction.L ? -1 : 1);
                if (newPosition >= 0 && newPosition < tape.Count)
                {
                    currentPosition = newPosition;
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

    #region Parsing
    public class ParseError
    {
        public readonly string message;
        public readonly ParseError causedBy;

        public ParseError(string message, ParseError causedBy = null)
        {
            this.message = message;
            this.causedBy = causedBy;
        }
    }
    #endregion

    public void PrintCommandList()
    {
        Console.WriteLine("Printing this Turing Machine's command list:");

        foreach (Command com in commands)
        {
            Console.WriteLine(com.ToString());
        }
    }

    public void WriteLineCurrentState()
    {
        // Print name
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(" Name: " + "UNNAMED123123123".PadLeft(15, ' ').Substring(0, 15) + ' ');

        // Space
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(' ');

        // Print current state
        Console.BackgroundColor = ConsoleColor.Cyan;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write(" State: " + currentState.PadLeft(15, ' ').Substring(0, 15) + ' ');

        // Space
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write(' ');

        // Print current position
        Console.BackgroundColor = ConsoleColor.Yellow;
        Console.Write(" Pos: " + currentPosition.ToString().PadLeft(15, ' ').Substring(0, 15) + ' ');

        // New line
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write('\n');

        // Tape
        PrintTape(20);

        // New line
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write('\n');
    }

    public void PrintTape(int size)
    {
        if (size < 1)
            return;

        int pos = currentPosition;
        int tapeLen = tape.Count;

        int leftPos = currentPosition - size;
        int rightPos = currentPosition + size;

        if (rightPos > tapeLen - 1)
        {
            // Move to left
            int offset = rightPos - (tapeLen - 1);
            rightPos -= offset;
            leftPos -= offset;
        }

        if (leftPos < 0)
        {
            // Move to right
            int offset = -leftPos;
            rightPos += offset;
            leftPos += offset;
        }

        // Left border
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        if (leftPos == 0)
            Console.Write("   ");
        else
            Console.Write("...");

        // Characters before current position
        for (int i = leftPos; i < currentPosition; i++)
            Console.Write(tape[i]);

        // Character at current position
        Console.BackgroundColor = ConsoleColor.DarkRed;
        Console.Write(tape[currentPosition]);

        // Characters after current position
        Console.BackgroundColor = ConsoleColor.Black;
        for (int i = currentPosition + 1; i < rightPos + 1 && i < tapeLen; i++)
            Console.Write(tape[i]);

        // Right border
        if (rightPos >= tapeLen - 1)
            Console.Write("   ");
        else
            Console.Write("...");
    }

}
